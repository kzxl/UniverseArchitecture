/**
 * EventBus — Sóng hấp dẫn (Gravitational Waves).
 * Giao tiếp gián tiếp giữa modules. Principle #5.
 */
export interface IEventBus {
  publish<T>(eventType: string, event: T): void;
  subscribe<T>(eventType: string, handler: (event: T) => void): void;
  unsubscribe<T>(eventType: string, handler: (event: T) => void): void;
}

export class EventBus implements IEventBus {
  private readonly handlers = new Map<string, Array<(event: any) => void>>();

  publish<T>(eventType: string, event: T): void {
    const handlers = this.handlers.get(eventType);
    if (!handlers) return;
    for (const handler of [...handlers]) handler(event);
  }

  subscribe<T>(eventType: string, handler: (event: T) => void): void {
    if (!this.handlers.has(eventType)) {
      this.handlers.set(eventType, []);
    }
    this.handlers.get(eventType)!.push(handler);
  }

  unsubscribe<T>(eventType: string, handler: (event: T) => void): void {
    const handlers = this.handlers.get(eventType);
    if (!handlers) return;
    const idx = handlers.indexOf(handler);
    if (idx >= 0) handlers.splice(idx, 1);
  }

  get typeCount(): number { return this.handlers.size; }
  get handlerCount(): number {
    let count = 0;
    for (const h of this.handlers.values()) count += h.length;
    return count;
  }
}
