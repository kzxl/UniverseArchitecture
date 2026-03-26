import type { IModule } from '../../core/IModule';
import type { IEventBus } from '../../core/EventBus';
import type { IModuleLifecycle } from '../../core/Lifecycle';

// ── Events ──
export interface CalculationPerformedEvent {
    operation: string;
    result: string;
}

export interface GreetingEvent {
    name: string;
    message: string;
}

/**
 * NotifierModule — Demo EventBus + Lifecycle.
 * Subscribes to events without importing source modules.
 */
export class NotifierModule implements IModule, IModuleLifecycle {
    private readonly notifications: string[] = [];

    constructor(private readonly eventBus: IEventBus) { }

    readonly name = 'notifier';
    readonly description = '🔔 Listens to events from other modules';
    readonly commands = ['history', 'count', 'clear'] as const;

    execute(command: string, _args: string[]): string {
        switch (command.toLowerCase()) {
            case 'history':
                if (this.notifications.length === 0) return '📭 No notifications yet.';
                return this.notifications.slice(-10)
                    .map((n, i) => `  ${i + 1}. ${n}`).join('\n');
            case 'count':
                return `📬 ${this.notifications.length} notification(s)`;
            case 'clear': {
                const count = this.notifications.length;
                this.notifications.length = 0;
                return `🗑️ Cleared ${count} notification(s)`;
            }
            default:
                return `❓ Unknown command: ${command}`;
        }
    }

    // Lifecycle
    async onInitializing(): Promise<void> { }
    async onInitialized(): Promise<void> {
        this.eventBus.subscribe<CalculationPerformedEvent>('CalculationPerformed', (e) => {
            this.notifications.push(`🧮 Calculation: ${e.operation} = ${e.result}`);
        });
        this.eventBus.subscribe<GreetingEvent>('Greeting', (e) => {
            this.notifications.push(`👋 Greeting: ${e.message}`);
        });
    }
    async onShuttingDown(): Promise<void> { }
    async onShutdown(): Promise<void> { }
}
