﻿using Confluent.Kafka;
using FrwkQuickWait.Domain.Constants;
using FrwkQuickWait.Domain.Entities;
using FrwkQuickWait.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace FrwkQuickWait.Service.Consumers
{
    public class UserConsumer : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly string topicName;
        private readonly ConsumerConfig consumerConfig;
        public UserConsumer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.topicName = Topics.topicNameUser;

            this.consumerConfig = new ConsumerConfig
            {
                BootstrapServers = CloudKarafka.Brokers,
                SaslUsername = CloudKarafka.Username,
                SaslPassword = CloudKarafka.Password,
                SaslMechanism = SaslMechanism.ScramSha256,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                EnableSslCertificateVerification = false,
                GroupId = $"{topicName}-group-0",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var task = Task.Run(() => ProcessQueue(stoppingToken), stoppingToken);

            return task;
        }

        private void ProcessQueue(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe($"{CloudKarafka.Prefix + topicName}");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        Task.Run(async () => { await InvokeService(consumeResult); }, stoppingToken);
                    }
                    catch (ConsumeException ex)
                    { }
                }
            }
            catch (OperationCanceledException ex)
            {
                consumer.Close();
            }
        }

        private async Task InvokeService(ConsumeResult<Ignore, string> message)
        {
            var mensagem = JsonConvert.DeserializeObject<MessageInput>(message.Message.Value);

            using var scope = serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetService<IUserService>();

            switch (mensagem.Method)
            {
                case MethodConstant.POST:
                    await userService.Save(JsonConvert.DeserializeObject<User>(mensagem.Content));
                    break;
                case MethodConstant.PUT:
                    userService.Update(JsonConvert.DeserializeObject<User>(mensagem.Content));
                    break;
                case MethodConstant.DELETE:
                    userService.Delete(JsonConvert.DeserializeObject<User>(mensagem.Content));
                    break;
                case MethodConstant.DELETEMANY:
                    userService.DeleteMany(JsonConvert.DeserializeObject<IEnumerable<User>>(mensagem.Content));
                    break;
            }
        }


    }
}
