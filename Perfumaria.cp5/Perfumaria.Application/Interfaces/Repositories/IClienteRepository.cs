using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Interfaces.Repositories;

public interface IClienteRepository : IRepository<Cliente, Guid>
{
    Task<Cliente?> ObterPorCpfCnpjAsync(string cpfCnpj);
    Task<Cliente?> ObterPorEmailAsync(string email);
    Task<IEnumerable<Cliente>> ObterComPedidosAsync();
}
