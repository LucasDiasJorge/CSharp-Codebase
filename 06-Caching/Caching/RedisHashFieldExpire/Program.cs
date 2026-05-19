using StackExchange.Redis;
using System;

internal static class Program
{
    private static void Main()
    {
        String connectionString = "localhost:6379";
        RedisKey hashKey = "demo:hashfieldexpire:product:1";
        RedisValue[] fieldsToExpire = new RedisValue[] { "field1" };
        TimeSpan fieldTtl = TimeSpan.FromMinutes(10);

        try
        {
            Console.WriteLine("Conectando ao Redis...");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);

            if (!redis.IsConnected)
            {
                Console.WriteLine("Conexao com Redis indisponivel.");
                return;
            }

            IDatabase database = redis.GetDatabase();

            database.KeyDelete(hashKey);
            database.HashSet(
                hashKey,
                new HashEntry[]
                {
                    new HashEntry("field1", "value1"),
                    new HashEntry("field2", "value2")
                });

            ExpireResult[] expireResults = database.HashFieldExpire(hashKey, fieldsToExpire, fieldTtl);

            Console.WriteLine("Hash criado com dois campos.");
            Console.WriteLine($"Aplicando TTL de {fieldTtl.TotalMinutes} minutos para o campo '{fieldsToExpire[0]}'...");
            Console.WriteLine($"Resultado do HashFieldExpire: {expireResults[0]}");
            Console.WriteLine($"Valor atual de field1: {database.HashGet(hashKey, "field1")}");
            Console.WriteLine($"Valor atual de field2: {database.HashGet(hashKey, "field2")}");
            Console.WriteLine("Dica: valide no redis-cli com HGETALL e HTTL para verificar expiração por campo.");

            redis.Close();
            Console.WriteLine("Conexao encerrada.");
        }
        catch (RedisServerException redisServerException)
        {
            Console.WriteLine("O servidor Redis nao aceitou HEXPIRE (base de HashFieldExpire).");
            Console.WriteLine("Use Redis 7.4+ para expiração por campo de hash.");
            Console.WriteLine($"Detalhe: {redisServerException.Message}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Erro ao executar exemplo: {exception.Message}");
        }
    }
}
