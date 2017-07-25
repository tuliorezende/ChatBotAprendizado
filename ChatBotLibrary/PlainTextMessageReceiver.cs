using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using System.Diagnostics;
using Business;
using Lime.Messaging.Contents;

namespace ChatBotLibrary
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;
        private DeputadosController dep;
        public PlainTextMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;

            dep = new DeputadosController();
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"From: {message.From} \tContent: {message.Content}");
            await _sender.SendMessageAsync("Aguarde, estamos pesquisando a respeito do deputado escolhido :)", message.From, cancellationToken);

            string NomeDeputado = message.Content.ToString().Split(':')[1].Trim();
            var document = ProcessaRetorno(NomeDeputado);

            if (document == null) await _sender.SendMessageAsync("N�o consegui identificar o deputado pesquisado. Poderia digitar o nome dele corretamente?", message.From, cancellationToken);
            else
            {
                await _sender.SendMessageAsync("Opa, parece que encontrei o que voc� quer!", message.From, cancellationToken);
                Thread.Sleep(2000);

                await _sender.SendMessageAsync(document, message.From, cancellationToken);
            }
        }

        private DocumentSelect ProcessaRetorno(string NomeDeputado)
        {
            Models.Deputado modelDeputado = dep.RetornaDeputadoEscolhido(NomeDeputado);

            if (modelDeputado == null) return null;

            var document = new DocumentSelect
            {
                Header = new DocumentContainer
                {
                    Value = new MediaLink
                    {
                        Text = $"RETRATO DO DEPUTADO {modelDeputado.nomeParlamentar}",
                        Type = MediaType.Parse("image/jpg"),
                        PreviewUri = new Uri(modelDeputado.urlFoto),
                        Uri = new Uri(modelDeputado.urlFoto),
                        Size = 400
                    }
                },
                Options = new DocumentSelectOption[] {
                    new DocumentSelectOption{Label=new DocumentContainer{Value= new PlainText{Text="Vota��es do Deputado" } },Order=1},
                    new DocumentSelectOption{Label=new DocumentContainer{Value= new PlainText{Text="Despesas do Deputado" } },Order=2},
                }
            };

            return document;
        }
    }
}