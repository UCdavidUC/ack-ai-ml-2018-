using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using SimpleEchoBot.Business;
using SimpleEchoBot.Models;
using Microsoft.Azure.Documents;

namespace SimpleEchoBot.Dialogs
{

    [Serializable]
    public class ProactiveDialog : IDialog<object>
    {
        [NonSerialized]
        Timer t;

        private readonly List<string> BinaryOptions = new List<string>() { "Sí", "No" };

        private Calificacion _calificacion;
        private Usuario usuario;

        private readonly List<string> MovieRatings = new List<string>()
        {
            "★★★★★ Excelente",
            "★★★★☆ Buena",
            "★★★☆☆ Regular",
            "★★☆☆☆ Mala",
            "★☆☆☆☆ Perdí mi tiempo",
            "No quiero calificarla"
        };

        private readonly List<string> MenuInicial = new List<string>()
        {
            "Buscar película",
            "Recomiéndame una película",
            "Comprar boletos",
            "Quejas y sugerencias", 
            "Mis puntos y beneficios",
            "Ayuda"
        };

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> response)
        {
            var message = await response;

            ConversationStarter.toId = message.From.Id;
            ConversationStarter.toName = message.From.Name;
            ConversationStarter.fromId = message.Recipient.Id;
            ConversationStarter.fromName = message.Recipient.Name;
            ConversationStarter.serviceUrl = message.ServiceUrl;
            ConversationStarter.channelId = message.ChannelId;
            ConversationStarter.conversationId = message.Conversation.Id;

            var url = HttpContext.Current.Request.Url;

            await context.PostAsync("¡Hola " + ConversationStarter.fromName + " te doy la bienvenida al paraíso del cine!");
            try
            {
                PromptDialog.Choice(context, RegistroChoice, BinaryOptions, "¿Te encuentras registrado en el portal?");
            } catch (Exception ex) {
                await context.PostAsync("Disculpa, esto es muy penoso ... Pero por el momento no podré atenderte");
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        public virtual async Task RegistroChoice(IDialogContext context, IAwaitable<string> response)
        {
            switch(await response) 
            {
                case "Sí":
                    await context.PostAsync("Dime, ¿Cuál es tu número de cliente?");
                    context.Wait(GetClientID);
                    break;
                case "No":
                    PromptDialog.Choice(context, RegisterForm, new List<string>() { "Registro", "Continuar" }, "Para hacer uso de la funcionalidad completa, te recomiendo registrate, yo te puedo ayudar con esto.");
                    break;
            }
        }

        private async Task RegisterForm(IDialogContext context, IAwaitable<string> response)
        {
            var res = await response;
        }

        public virtual async Task GetClientID(IDialogContext context, IAwaitable<IMessageActivity> response)
        {
            var clientID = await response;
            usuario = new Usuario();
            // Validación de usuario en base de datos.
            try
            {
                using (Data.Database db = new Data.Database())
                {
                    usuario = await db.RetrieveUser(clientID.Text);
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            

            await context.PostAsync($"Te confirmo que tu número de cliente es: {clientID.Text}. Es bueno tenerte de vuelta, ¿Dime que te pareció la película {usuario.MovieName}?");
            _calificacion = new Calificacion()
            {
                UserID = clientID.Text,
                MovieID = usuario.MovieName
            };
            using (BingSearch bs = new BingSearch())
            {
                var message = context.MakeMessage();
                var attachment = GetThumbnailCard(usuario.MovieName, usuario.Rating.ToString(), string.Empty, await bs.BuscarImagen(usuario.MovieName));
                message.Attachments.Add(attachment);
                await context.PostAsync(message);
            }
            PromptDialog.Choice(context, RateMovie, MovieRatings, "Selecciona la puntuación con la que calificarías la película");
        }

        private Microsoft.Bot.Connector.Attachment GetThumbnailCard(string title, string subtitle, string text, string thumbnail_url)
        {
            string rating = string.Empty;
            switch(Int32.Parse(subtitle))
            {
                case 0:
                    rating = "☆☆☆☆☆";
                    break;
                case 1:
                case 2:
                    rating = "★☆☆☆☆";
                    break;
                case 3:
                case 4:
                    rating = "★★☆☆☆";
                    break;
                case 5:
                case 6:
                    rating = "★★★☆☆";
                    break;
                case 7:
                case 8:
                    rating = "★★★★☆";
                    break;
                case 9:
                case 10:
                    rating = "★★★★★";
                    break;
                default:
                    rating = "☆☆☆☆☆";
                    break;
            }
            var card = new ThumbnailCard()
            {
                Title = title,
                Subtitle = $"Rating: {subtitle.ToString()} {rating}",
                Images = new List<CardImage>() { new CardImage(thumbnail_url) },
            };
            return card.ToAttachment();
        }

        private async Task RateMovie(IDialogContext context, IAwaitable<string> response)
        {
            var res = await response;
            string thankYouMsg = "Gracias, tu opinión es muy valiosa para nosotros.";
            string continueMsg = "¿En qué más puedo ayudarte?";
            if (res != MovieRatings[5])
            {
                // TODO: Insert movie rating into database.
                int rating = 0;
                if (res != MovieRatings[0])
                {
                    rating = 10;
                } else if (res != MovieRatings[1])
                {
                    rating = 7;
                } else if (res != MovieRatings[2])
                {
                    rating = 5;
                } else if (res != MovieRatings[3])
                {
                    rating = 3;
                } else if (res != MovieRatings[4])
                {
                    rating = 1;
                }
                _calificacion.Rating = rating;
                await context.PostAsync(string.Format("{0} {1}", thankYouMsg, continueMsg));
            }
            PromptDialog.Choice(context, SeleccionMenu, MenuInicial, "¿En qué más te puedo ayudar?");
        }

        private async Task SeleccionMenu(IDialogContext context, IAwaitable<string> response)
        {
            var seleccion = await response;
            var reply = context.MakeMessage();
            if (!seleccion.Equals(MenuInicial[1]))
            {
                await context.PostAsync($"Disculpa, estoy aprendiendo y por el momento no tengo la funcionalidad de {seleccion}.");
                PromptDialog.Choice(context, EndDialog, MenuInicial, "¿Te puedo ayudar en algo más?");
            } else
            {
                List<string> recomendacion = new List<string>();
                try
                {
                    using (Recomendador recomendador = new Recomendador())
                    {
                        recomendacion = await recomendador.InvokeRequestResponseService(_calificacion.UserID, _calificacion.MovieID, _calificacion.Rating.ToString());
                    }
                } catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
                try
                {
                    if (recomendacion.Count > 1)
                    {
                        await context.PostAsync("Te voy a hacer una oferta que no podrás rechazar.");
                        reply.Attachments.Add(new Microsoft.Bot.Connector.Attachment()
                        {
                            ContentUrl = "https://media.giphy.com/media/26h0pkvcgnFIpvU1a/giphy.gif",
                            ContentType = "image/gif",
                            Name = "El Padrino"
                        });
                        await context.PostAsync(reply);
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments = await GetCarousel(recomendacion);
                        await context.PostAsync(reply);
                    }
                    else
                    {
                        await context.PostAsync("Mmmm, has visto pocas películas. ¡Te recomiendo ver muchas más!, de esta forma me ayudas a conocerte mejor y poder ofrecerte películas que seguramente te encantarán.");
                        context.Wait(MessageReceivedAsync);
                    }
                } catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }
                MenuInicial.Add("Salir");
                PromptDialog.Choice(context, EndDialog, MenuInicial, "¿Te puedo ayudar en algo más?");
            }
        }

        private async Task EndDialog(IDialogContext context, IAwaitable<string> response)
        {
            var seleccion = await response;
            var reply = context.MakeMessage();
            if (seleccion.ToLower().Contains("nada") || seleccion.ToLower().Contains("no") || seleccion.Equals("Salir"))
            {
                await context.PostAsync("Fue un gusto atenderte, que la fuerza te acompañe.");
                reply.Attachments.Add(new Microsoft.Bot.Connector.Attachment()
                {
                    ContentUrl = "https://thumbs.gfycat.com/PhonyMarvelousEchidna-small.gif",
                    ContentType = "image/gif",
                    Name = "May the Fourth be with you."
                });
                await context.PostAsync(reply);
            } else if (!seleccion.Equals(MenuInicial[1]))
            {
                await context.PostAsync($"Disculpa, estoy aprendiendo y por el momento no tengo la funcionalidad de {seleccion}.");
                PromptDialog.Choice(context, EndDialog, MenuInicial, "¿Te puedo ayudar en algo más?");
            }
            else
            {
                List<string> recomendacion = new List<string>();
                try
                {
                    using (Recomendador recomendador = new Recomendador())
                    {
                        recomendacion = await recomendador.InvokeRequestResponseService(_calificacion.UserID, _calificacion.MovieID, _calificacion.Rating.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
                try
                {
                    if (recomendacion.Count > 1)
                    {
                        await context.PostAsync("Te voy a hacer una oferta que no podrás rechazar.");
                        reply.Attachments.Add(new Microsoft.Bot.Connector.Attachment()
                        {
                            ContentUrl = "https://media.giphy.com/media/26h0pkvcgnFIpvU1a/giphy.gif",
                            ContentType = "image/gif",
                            Name = "El Padrino"
                        });
                        await context.PostAsync(reply);
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments = await GetCarousel(recomendacion);
                        await context.PostAsync(reply);
                    }
                    else
                    {
                        await context.PostAsync("Mmmm, has visto pocas películas. ¡Te recomiendo ver muchas más!, de esta forma me ayudas a conocerte mejor y poder ofrecerte películas que seguramente te encantarán.");
                        context.Wait(MessageReceivedAsync);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                }

                PromptDialog.Choice(context, EndDialog, MenuInicial, "¿Te puedo ayudar en algo más?");
            }
        }

        private async Task<IList<Microsoft.Bot.Connector.Attachment>> GetCarousel(List<string> recomendacion)
        {
            List<Microsoft.Bot.Connector.Attachment> attachments = new List<Microsoft.Bot.Connector.Attachment>();
            for(int i = 1; i < recomendacion.Count; i++)
            {
                using (BingSearch videoSearch = new BingSearch())
                {
                    string video = await videoSearch.BuscarVideo($"{recomendacion[i]} trailer");
                    string image = await videoSearch.BuscarImagen($"{recomendacion[i]}");
                    attachments.Add(GetVideoCard(recomendacion[i], string.Empty, string.Empty, image, video));
                }
            }
            return attachments;
        }
        
        private static Microsoft.Bot.Connector.Attachment GetVideoCard(string title, string subtitle, string text, string image, string trailer_url)
        {
            var card = new VideoCard()
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Media = new List<MediaUrl>() { new MediaUrl(trailer_url) },
                Autostart = false,
                Shareable = true,
                Autoloop = false,
                Image = new ThumbnailUrl(image),
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.PlayVideo, "Ver trailer", trailer_url, trailer_url),
                    new CardAction(ActionTypes.OpenUrl, "Ver horarios", string.Empty, "https://moviesrecombot.azurewebsites.net"),
                    new CardAction(ActionTypes.OpenUrl, "Comprar boletos", string.Empty, "https://moviesrecombot.azurewebsites.net")
                }
            };
            return card.ToAttachment();
        }
    }
}