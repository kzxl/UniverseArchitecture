import type { IModule } from '../../core/IModule';

/**
 * Greeter Module — Module thứ 2 trong vũ trụ.
 * Chứng minh thêm module mới = thêm 1 file + 1 dòng register.
 */
export class GreeterModule implements IModule {
  readonly name = 'greeter';
  readonly description = 'Greeting messages (hello, goodbye)';
  readonly commands = ['hello', 'goodbye'] as const;

  execute(command: string, args: string[]): string {
    const name = args.length > 0 ? args.join(' ') : 'World';

    switch (command.toLowerCase()) {
      case 'hello':   return `👋 Hello, ${name}! Welcome to the Universe!`;
      case 'goodbye': return `🌙 Goodbye, ${name}! See you among the stars!`;
      default: return `Unknown command '${command}'. Available: ${this.commands.join(', ')}`;
    }
  }
}
