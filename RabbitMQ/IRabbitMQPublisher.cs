﻿namespace IdentityService.RabbitMQ
{
    public interface IRabbitMQPublisher<T>
    {
        public void Publish(T message, params string[] routingKeys);
    }
}