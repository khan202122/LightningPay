﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using LightningPay.Infrastructure.Api;

namespace LightningPay.Clients.Lnd
{
    /// <summary>
    ///   LND events Listener
    /// </summary>
    public class LndListener : ApiServiceBase, ILightningListener
    {
        private bool clientInternalBuilt = false;

        private readonly IEventSubscriptionsManager eventSubscriptionsManager;

        private CancellationTokenSource cts = new CancellationTokenSource();

        private Task listenTask;

        /// <summary>Initializes a new instance of the <see cref="LndListener" /> class.</summary>
        /// <param name="client"></param>
        /// <param name="eventSubscriptionsManager">The event subscriptions manager.</param>
        /// <param name="options">The options.</param>
        public LndListener(HttpClient client, 
            IEventSubscriptionsManager eventSubscriptionsManager,
            LndOptions options) : base(options.Address.ToBaseUrl(), client, LndClient.BuildAuthentication(options))
        {
            this.eventSubscriptionsManager = eventSubscriptionsManager;
        }

        /// <summary>Subscribes to an event.</summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Subscribe<TEvent, THandler>()
            where TEvent : LightningEvent
            where THandler : ILightningEventHandler<TEvent>
        {
            this.eventSubscriptionsManager.AddSubscription<TEvent, THandler>();
        }

        /// <summary>Subscribes the specified handler.</summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler.</param>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : LightningEvent
        {
            throw new NotImplementedException();
        }

        /// <summary>Unsubscribes to an event.</summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        public void Unsubscribe<TEvent, THandler>()
            where TEvent : LightningEvent
            where THandler : ILightningEventHandler<TEvent>
        {
            this.eventSubscriptionsManager.RemoveEventSubscription<TEvent, THandler>();
        }

        /// <summary>Starts listening the events.</summary>
        public Task StartListening()
        {
            this.listenTask = this.ListenLoop();
            return Task.CompletedTask;
        }

        private async Task ListenLoop()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{this.baseUrl}/v1/invoices/subscribe");
            await this.authentication.AddAuthentication(this.httpClient, request);
            var response = await this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            var body = await response.Content.ReadAsStreamAsync();
            using (var reader = new StreamReader(body))
            {
                while (!this.cts.IsCancellationRequested)
                {
                    string line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        if (line.Contains("\"result\":"))
                        {

                        }
                        else if (line.Contains("\"error\":"))
                        {

                        }
                        else
                        {
                            throw new LightningPayException("Unknown result from LND", LightningPayException.ErrorCode.INTERNAL_ERROR);
                        }
                    }
                }
            }
        }

            /// <summary>Stops listening the events.</summary>
        public Task StopListening()
        {
            this.cts.Cancel();

            return Task.CompletedTask;
        }

        /// <summary>Instanciate a new LND Listener.</summary>
        /// <param name="address">The address of the lnd api api.</param>
        /// <param name="macaroonHexString">The macaroon hexadecimal string.</param>
        /// <param name="macaroonBytes">The macaroon bytes.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="eventSubscriptionsManager">Event subscription manager to use.</param>
        /// <returns>LND Listener</returns>
        public static LndListener New(string address,
            string macaroonHexString = null,
            byte[] macaroonBytes = null,
            HttpClient httpClient = null,
            IEventSubscriptionsManager eventSubscriptionsManager = null)
        {
            bool clientInternalBuilt = false;

            if (httpClient == null)
            {
                httpClient = new HttpClient();
                clientInternalBuilt = true;
            }

            if (eventSubscriptionsManager == null)
            {
                eventSubscriptionsManager = new InMemoryEventSubscriptionsManager();
            }

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri))
            {
                throw new LightningPayException($"Invalid uri format for LND Client : {address}",
                    LightningPayException.ErrorCode.BAD_CONFIGURATION);
            }

            LndListener listener = new LndListener(httpClient, eventSubscriptionsManager, new LndOptions()
            {
                Address = new Uri(address),
                Macaroon = macaroonBytes ?? macaroonHexString.HexStringToByteArray()
            });

            listener.clientInternalBuilt = clientInternalBuilt;

            return listener;
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if(!cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            if (this.clientInternalBuilt)
            {
                this.httpClient?.Dispose();
            }
        }
    }
}
