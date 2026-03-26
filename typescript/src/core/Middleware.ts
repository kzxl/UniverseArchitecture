/**
 * ModuleContext — Ngữ cảnh cho mỗi dispatch, đi qua middleware pipeline.
 */
export interface ModuleContext {
    readonly moduleName: string;
    readonly command: string;
    readonly args: string[];
    result?: string;
    items: Record<string, any>;
    timestamp: number;
}

/**
 * Middleware — Lực hấp dẫn (Gravity). Principle #7.
 * Chain of Responsibility pattern.
 */
export interface IMiddleware {
    invoke(context: ModuleContext, next: () => void): void;
}

/** Logging Middleware */
export class LoggingMiddleware implements IMiddleware {
    readonly logs: string[] = [];

    invoke(context: ModuleContext, next: () => void): void {
        const ts = new Date().toISOString().slice(11, 23);
        let log = `[LOG] ${ts} → ${context.moduleName}.${context.command}(${context.args.join(', ')})`;
        next();
        log += ` → ${context.result}`;
        this.logs.push(log);
    }
}

/** Timing Middleware */
export class TimingMiddleware implements IMiddleware {
    invoke(context: ModuleContext, next: () => void): void {
        const start = performance.now();
        next();
        context.items['elapsed_ms'] = performance.now() - start;
    }
}

/** Error Handling Middleware */
export class ErrorHandlingMiddleware implements IMiddleware {
    invoke(context: ModuleContext, next: () => void): void {
        try {
            next();
        } catch (e: any) {
            context.result = `❌ Error: ${e.message ?? e}`;
        }
    }
}
