using MauiAppTempoAgora.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace MauiAppTempoAgora.Services
{
    public class DataService
    {
        public static async Task<Tempo?> GetPrevisao(string cidade)
        {
            Tempo? t = null;
            string chave = "6135072afe7f6cec1537d5cb08a5a1a2";
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cidade}&units=metric&appid={chave}&lang=pt_br";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage resp = await client.GetAsync(url);

                    if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                        throw new Exception("Cidade não encontrada.");

                    if (!resp.IsSuccessStatusCode)
                        throw new Exception("Erro ao buscar dados do clima.");

                    string json = await resp.Content.ReadAsStringAsync();
                    var rascunho = JObject.Parse(json);

                    DateTime agora = DateTime.Now;
                    DateTime sunrise = DateTimeOffset.FromUnixTimeSeconds((long)rascunho["sys"]["sunrise"]).ToLocalTime().DateTime;
                    DateTime sunset = DateTimeOffset.FromUnixTimeSeconds((long)rascunho["sys"]["sunset"]).ToLocalTime().DateTime;

                    t = new Tempo
                    {
                        lat = (double)rascunho["coord"]["lat"],
                        lon = (double)rascunho["coord"]["lon"],
                        description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((string)rascunho["weather"][0]["description"]),
                        main = TraduzirMain((string)rascunho["weather"][0]["main"]),
                        temp_min = (double)rascunho["main"]["temp_min"],
                        temp_max = (double)rascunho["main"]["temp_max"],
                        speed = (double)rascunho["wind"]["speed"],
                        visibility = (int)rascunho["visibility"],
                        sunrise = sunrise.ToString("dd/MM/yyyy HH:mm"),
                        sunset = sunset.ToString("dd/MM/yyyy HH:mm"),
                    };
                }
                catch (HttpRequestException)
                {
                    throw new Exception("Sem conexão com a internet.");
                }
            }

            return t;
        }

        private static string TraduzirMain(string main)
        {
            return main switch
            {
                "Clear" => "Céu limpo",
                "Clouds" => "Nublado",
                "Rain" => "Chuva",
                "Drizzle" => "Garoa",
                "Thunderstorm" => "Trovoadas",
                "Snow" => "Neve",
                "Mist" => "Névoa",
                _ => main // se não encontrar, mantém original
            };
        }
    }
}

