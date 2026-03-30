import type { IModule } from '../../core/IModule';
import type { EventBus } from '../../core/EventBus';

/**
 * SalesModule — Tạo đơn hàng, publish OrderCreatedEvent.
 * KHÔNG import InventoryModule — chỉ publish event (Principle #5).
 */
export class SalesModule implements IModule {
  readonly name = 'sales';
  readonly description = '🛒 Sales order management';
  readonly commands = ['create-order', 'list-orders'] as const;

  private orders: Array<{ orderId: string; productId: string; qty: number }> = [];
  private orderCounter = 0;

  constructor(private readonly eventBus: EventBus) {}

  execute(command: string, args: string[]): string {
    switch (command.toLowerCase()) {
      case 'create-order': return this.createOrder(args);
      case 'list-orders': return this.listOrders();
      default: return `❓ Unknown command: ${command}`;
    }
  }

  private createOrder(args: string[]): string {
    if (args.length < 2) return 'Error: usage: sales create-order <productId> <quantity>';
    const pid = args[0].toUpperCase();
    const qty = parseInt(args[1]);
    if (isNaN(qty)) return `Error: invalid quantity '${args[1]}'`;

    this.orderCounter++;
    const orderId = `ORD-${String(this.orderCounter).padStart(4, '0')}`;
    this.orders.push({ orderId, productId: pid, qty });

    // Publish event — InventoryModule và NotifierModule sẽ react
    this.eventBus.publish('OrderCreated', { orderId, productId: pid, quantity: qty });

    return `🛒 Order ${orderId} created: ${qty}x ${pid}`;
  }

  private listOrders(): string {
    if (this.orders.length === 0) return '📭 No orders yet.';
    return this.orders
      .map((o, i) => `  ${i + 1}. ${o.orderId}: ${o.qty}x ${o.productId}`)
      .join('\n');
  }
}
