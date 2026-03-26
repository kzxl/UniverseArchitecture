import type { IModule } from './IModule';

/**
 * ModuleLifecycle — Optional lifecycle hooks (Star Lifecycle).
 * Modules implementing this will get init/shutdown callbacks.
 */
export interface IModuleLifecycle {
    onInitializing(): Promise<void> | void;
    onInitialized(): Promise<void> | void;
    onShuttingDown(): Promise<void> | void;
    onShutdown(): Promise<void> | void;
}

/** Type guard: check if module has lifecycle hooks. */
export function hasLifecycle(module: IModule): module is IModule & IModuleLifecycle {
    return 'onInitializing' in module && 'onInitialized' in module;
}
