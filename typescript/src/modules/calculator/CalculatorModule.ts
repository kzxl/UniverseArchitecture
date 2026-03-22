import type { IModule } from '../../core/IModule';

/**
 * Calculator Module — Một "ngôi sao" trong vũ trụ.
 * Tự chứa, tuân thủ IModule interface.
 */
export class CalculatorModule implements IModule {
  readonly name = 'calculator';
  readonly description = 'Basic arithmetic operations (add, sub, mul, div)';
  readonly commands = ['add', 'sub', 'mul', 'div'] as const;

  execute(command: string, args: string[]): string {
    if (args.length < 2) {
      return 'Error: need 2 numbers. Usage: calculator add 1 2';
    }

    const a = parseFloat(args[0]);
    const b = parseFloat(args[1]);
    if (isNaN(a) || isNaN(b)) {
      return `Error: invalid numbers '${args[0]}' '${args[1]}'`;
    }

    switch (command.toLowerCase()) {
      case 'add': return `${a} + ${b} = ${a + b}`;
      case 'sub': return `${a} - ${b} = ${a - b}`;
      case 'mul': return `${a} * ${b} = ${a * b}`;
      case 'div': return b === 0 ? 'Error: division by zero' : `${a} / ${b} = ${a / b}`;
      default: return `Unknown command '${command}'. Available: ${this.commands.join(', ')}`;
    }
  }
}
