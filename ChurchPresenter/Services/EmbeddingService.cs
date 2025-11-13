using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Text;
using System.Text.RegularExpressions;

namespace ChurchPresenter.Services;

public class EmbeddingService : IDisposable
{
    private readonly InferenceSession _session;
    private readonly Dictionary<string, int> _vocab;
    private const int MaxSequenceLength = 128;
    private const int UnknownToken = 100; // [UNK]
    private const int ClsToken = 101; // [CLS]
    private const int SepToken = 102; // [SEP]

    public EmbeddingService()
    {
        var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "ML", "model.onnx");
        var vocabPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "ML", "vocab.txt");

        if (!File.Exists(modelPath))
            throw new FileNotFoundException($"Modelo no encontrado: {modelPath}");
        
        if (!File.Exists(vocabPath))
            throw new FileNotFoundException($"Vocabulario no encontrado: {vocabPath}");

        _session = new InferenceSession(modelPath);
        _vocab = LoadVocabulary(vocabPath);
    }

    private Dictionary<string, int> LoadVocabulary(string vocabPath)
    {
        var vocab = new Dictionary<string, int>();
        var lines = File.ReadAllLines(vocabPath);
        for (int i = 0; i < lines.Length; i++)
        {
            vocab[lines[i]] = i;
        }
        return vocab;
    }

    private List<string> Tokenize(string text)
    {
        // Normalizar texto: remover acentos y convertir a minúsculas
        text = RemoveDiacritics(text.ToLowerInvariant());
        
        var tokens = new List<string>();
        
        // Dividir por espacios primero
        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            // Limpiar puntuación al inicio y final
            var cleanWord = word.Trim('.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']');
            if (string.IsNullOrWhiteSpace(cleanWord)) continue;
            
            // WordPiece tokenization
            if (_vocab.ContainsKey(cleanWord))
            {
                // Palabra completa existe en vocabulario
                tokens.Add(cleanWord);
            }
            else
            {
                // Intentar dividir en subpalabras (WordPiece)
                var subTokens = WordPieceTokenize(cleanWord);
                tokens.AddRange(subTokens);
            }
        }
        
        return tokens;
    }

    private List<string> WordPieceTokenize(string word)
    {
        var tokens = new List<string>();
        int start = 0;
        
        while (start < word.Length)
        {
            int end = word.Length;
            string? foundToken = null;
            
            // Buscar la subpalabra más larga que exista en el vocabulario
            while (start < end)
            {
                string substr = start == 0 
                    ? word.Substring(start, end - start)
                    : "##" + word.Substring(start, end - start);
                
                if (_vocab.ContainsKey(substr))
                {
                    foundToken = substr;
                    break;
                }
                end--;
            }
            
            if (foundToken != null)
            {
                tokens.Add(foundToken);
                start = end;
            }
            else
            {
                // No se encontró, usar token desconocido
                tokens.Add("[UNK]");
                start++;
            }
        }
        
        return tokens;
    }

    public float[] GenerateEmbedding(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new float[384]; // Embedding vacío

        // Tokenizar el texto
        var tokens = Tokenize(text);
        
        // Debug: mostrar tokens generados (solo para las primeras llamadas)
        if (text.Length < 100) // Solo para textos cortos (queries)
        {
            Console.WriteLine($"Texto: '{text}'");
            Console.WriteLine($"Tokens: {string.Join(" | ", tokens.Take(20))}");
        }
        
        // Convertir tokens a IDs
        var tokenIds = new List<int>();
        foreach (var token in tokens)
        {
            if (_vocab.TryGetValue(token, out int id))
            {
                tokenIds.Add(id);
            }
            else if (token == "[UNK]" || token.StartsWith("##"))
            {
                tokenIds.Add(UnknownToken);
            }
            else
            {
                tokenIds.Add(UnknownToken);
            }
        }

        // Truncar a MaxSequenceLength - 2 (para [CLS] y [SEP])
        if (tokenIds.Count > MaxSequenceLength - 2)
            tokenIds = tokenIds.Take(MaxSequenceLength - 2).ToList();

        // Añadir tokens especiales [CLS] y [SEP]
        var inputIds = new List<long> { ClsToken };
        inputIds.AddRange(tokenIds.Select(id => (long)id));
        inputIds.Add(SepToken);

        // Rellenar con ceros hasta MaxSequenceLength
        while (inputIds.Count < MaxSequenceLength)
            inputIds.Add(0);

        // Crear attention mask (1 para tokens reales, 0 para padding)
        var attentionMask = inputIds.Select(id => id != 0 ? 1L : 0L).ToArray();

        // Crear tensores
        var inputIdsTensor = new DenseTensor<long>(inputIds.ToArray(), new[] { 1, MaxSequenceLength });
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, MaxSequenceLength });
        var tokenTypeIdsTensor = new DenseTensor<long>(new long[MaxSequenceLength], new[] { 1, MaxSequenceLength });

        // Crear inputs para el modelo
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
        };

        // Ejecutar el modelo
        using var results = _session.Run(inputs);
        
        // El modelo devuelve: last_hidden_state con shape [batch_size, sequence_length, hidden_size]
        // En nuestro caso: [1, 128, 384]
        var output = results.First().AsEnumerable<float>().ToArray();
        
        // Mean pooling: promediar los embeddings de todos los tokens (excluyendo padding)
        var embeddingSize = 384; // all-MiniLM-L6-v2 usa 384 dimensiones
        var embedding = new float[embeddingSize];
        
        // Contar tokens reales (sin padding)
        int realTokenCount = attentionMask.Count(m => m == 1);
        
        // Sumar embeddings de todos los tokens reales
        for (int i = 0; i < realTokenCount; i++)
        {
            for (int j = 0; j < embeddingSize; j++)
            {
                int index = i * embeddingSize + j;
                if (index < output.Length)
                {
                    embedding[j] += output[index];
                }
            }
        }
        
        // Promediar
        for (int j = 0; j < embeddingSize; j++)
        {
            embedding[j] /= realTokenCount;
        }

        // Normalizar el embedding
        return Normalize(embedding);
    }

    private float[] Normalize(float[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude == 0)
            return vector;

        return vector.Select(x => (float)(x / magnitude)).ToArray();
    }

    public static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            throw new ArgumentException("Los vectores deben tener la misma dimensión");

        return TensorPrimitives.CosineSimilarity(
            new ReadOnlySpan<float>(vector1),
            new ReadOnlySpan<float>(vector2)
        );
    }

    private static string RemoveDiacritics(string text)
    {
        // Normalizar el texto a FormD (separar caracteres base de diacríticos)
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            // Mantener solo caracteres que no sean marcas diacríticas
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Normalizar de vuelta a FormC (forma compuesta)
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
