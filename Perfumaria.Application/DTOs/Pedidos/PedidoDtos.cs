using System.ComponentModel.DataAnnotations;

namespace Perfumaria.Application.DTOs.Pedidos;

/// <summary>Um item de pedido no payload de criação.</summary>
public class ItemPedidoRequestDto
{
    [Required]
    public Guid ProdutoId { get; set; }

    [Range(1, 1000)]
    public int Quantidade { get; set; }

    [Range(0, 100)]
    public decimal DescontoPercentual { get; set; } = 0;
}

/// <summary>Payload de entrada para criação de um pedido, com seus itens.</summary>
public class PedidoRequestDto
{
    [Required]
    public Guid ClienteId { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    [Required, MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
    public List<ItemPedidoRequestDto> Itens { get; set; } = new();
}

/// <summary>Um item de pedido no payload de saída.</summary>
public class ItemPedidoResponseDto
{
    public Guid ProdutoId { get; set; }
    public string? ProdutoNome { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal DescontoPercentual { get; set; }
}

/// <summary>Payload de saída representando um pedido, com seus itens.</summary>
public class PedidoResponseDto
{
    public Guid Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public string? Observacoes { get; set; }
    public Guid ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public List<ItemPedidoResponseDto> Itens { get; set; } = new();
}
