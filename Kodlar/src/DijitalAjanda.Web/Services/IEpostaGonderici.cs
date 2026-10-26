namespace DijitalAjanda.Web.Services;

public interface IEpostaGonderici
{
    Task GonderAsync(string alici, string konu, string htmlIcerik);
}
