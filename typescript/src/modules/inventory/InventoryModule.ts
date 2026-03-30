import type { IModule } from '../../core/IModule';
import type { EventBus } from '../../core/EventBus';

/**
 * InventoryModule — Quản lý kho (Data Sovereignty).
 * Demo Principle #6: Data Sovereignty + #5: EventBus communication.
 */
export class InventoryModule implements IModule {
  readonly name = 'inventory';
  readonly description = '📦 Inventory management (stock tracking)';
  readonly commands = ['check', 'add', 'deduct', 'list'] as const;

  // Simulated stock database (owned exclusively by this module)
  private stock: Map<string, number> = new Map([
    ['PROD-001', 100],
    ['PROD-002', 50],
    ['PROD-003', 200],
  ]);

  constructor(private readonly eventBus: EventBus) {}

  execute(command: string, args: string[]): string {
    switch (command.toLowerCase()) {
      case 'check': return this.checkStock(args);
      case 'add': return this.addStock(args);
      case 'deduct': return this.deductStock(args);
      case 'list': return this.listStock();
      default: return `❓ Unknown command: ${command}`;
    }
  }

  private checkStock(args: string[]): string {
    if (args.length < 1) return 'Error: usage: inventory check <productId>';
    const pid = args[0].toUpperCase();
    const qty = this.stock.get(pid);
    if (qty === undefined) return `❌ Product '${pid}' not found`;
    return `📦 ${pid}: ${qty} units in stock`;
  }

  private addStock(args: string[]): string {
    if (args.length < 2) return 'Error: usage: inventory add <productId> <quantity>';
    const pid = args[0].toUpperCase();
    const qty = parseInt(args[1]);
    if (isNaN(qty)) return `Error: invalid quantity '${args[1]}'`;
    this.stock.set(pid, (this.stock.get(pid) ?? 0) + qty);
    this.eventBus.publish('StockChanged', { productId: pid, quantity: qty, action: 'added' });
    return `✅ Added ${qty} units to ${pid}. New stock: ${this.stock.get(pid)}`;
  }

  deductStock(args: string[]): string {
    if (args.length < 2) return 'Error: usage: inventory deduct <productId> <quantity>';
    const pid = args[0].toUpperCase();
    const qty = parseInt(args[1]);
    if (isNaN(qty)) return `Error: invalid quantity '${args[1]}'`;
    const current = this.stock.get(pid) ?? 0;
    if (current < qty) return `❌ Insufficient stock for ${pid}. Available: ${current}`;
    this.stock.set(pid, current - qty);
    this.eventBus.publish('StockChanged', { productId: pid, quantity: qty, action: 'deducted' });
    return `📤 Deducted ${qty} units from ${pid}. Remaining: ${this.stock.get(pid)}`;
  }

  private listStock(): string {
    if (this.stock.size === 0) return '📭 No products in stock.';
    const lines: string[] = [];
    let i = 1;
    for (const [pid, qty] of this.stock) {
      lines.push(`  ${i}. ${pid}: ${qty} units`);
      i++;
    }
    return lines.join('\n');
  }
}
