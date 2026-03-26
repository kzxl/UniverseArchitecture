import type { IModule } from './IModule';
import type { IMiddleware, ModuleContext } from './Middleware';
import { EventBus } from './EventBus';
import { hasLifecycle } from './Lifecycle';

/**
 * Registry — Trung tâm đăng ký module.
 * Upgraded: EventBus, Middleware pipeline, Lifecycle hooks.
 */
export class ModuleRegistry {
  private readonly modules = new Map<string, IModule>();
  private readonly middlewares: IMiddleware[] = [];
  readonly eventBus = new EventBus();

  /** Đăng ký 1 module. */
  register(module: IModule): void {
    const key = module.name.toLowerCase();
    if (this.modules.has(key)) {
      throw new Error(`Module '${module.name}' already registered.`);
    }
    this.modules.set(key, module);
  }

  /** Đăng ký module với lifecycle hooks. */
  async registerAsync(module: IModule): Promise<void> {
    if (hasLifecycle(module)) await module.onInitializing();
    this.register(module);
    if (hasLifecycle(module)) await module.onInitialized();
  }

  /** Thêm middleware vào pipeline. FIFO. */
  addMiddleware(mw: IMiddleware): void {
    this.middlewares.push(mw);
  }

  /** Dispatch command qua middleware pipeline. */
  dispatch(moduleName: string, command: string, args: string[]): string {
    const module = this.modules.get(moduleName.toLowerCase());
    if (!module) {
      const available = [...this.modules.keys()].join(', ');
      throw new Error(`Module '${moduleName}' not found. Available: ${available}`);
    }

    if (this.middlewares.length === 0) {
      return module.execute(command, args);
    }

    const context: ModuleContext = {
      moduleName, command, args,
      items: {},
      timestamp: Date.now(),
    };

    let index = -1;
    const next = (): void => {
      index++;
      if (index < this.middlewares.length) {
        this.middlewares[index].invoke(context, next);
      } else {
        context.result = module.execute(command, args);
      }
    };
    next();

    return context.result ?? module.execute(command, args);
  }

  /** Shutdown all modules with lifecycle. */
  async shutdown(): Promise<void> {
    for (const module of this.modules.values()) {
      if (hasLifecycle(module)) {
        await module.onShuttingDown();
        await module.onShutdown();
      }
    }
  }

  getAll(): ReadonlyMap<string, IModule> { return this.modules; }
  get count(): number { return this.modules.size; }
  get middlewareCount(): number { return this.middlewares.length; }
}
