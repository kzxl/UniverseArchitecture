import type { IModule } from './IModule';

/**
 * Registry — Trung tâm đăng ký module.
 * Module tự đăng ký, Core không biết chi tiết module.
 * Map-based lookup, case-insensitive.
 */
export class ModuleRegistry {
  private readonly modules = new Map<string, IModule>();

  /** Đăng ký 1 module. Trùng tên → throw. */
  register(module: IModule): void {
    const key = module.name.toLowerCase();
    if (this.modules.has(key)) {
      throw new Error(`Module '${module.name}' already registered.`);
    }
    this.modules.set(key, module);
  }

  /** Dispatch command tới module phù hợp. */
  dispatch(moduleName: string, command: string, args: string[]): string {
    const module = this.modules.get(moduleName.toLowerCase());
    if (!module) {
      const available = [...this.modules.keys()].join(', ');
      throw new Error(`Module '${moduleName}' not found. Available: ${available}`);
    }
    return module.execute(command, args);
  }

  /** Lấy tất cả modules. */
  getAll(): ReadonlyMap<string, IModule> {
    return this.modules;
  }

  /** Số lượng modules. */
  get count(): number {
    return this.modules.size;
  }
}
