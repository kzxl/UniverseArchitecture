package middleware

import (
	"fmt"
	"time"

	"universe-demo/core"
)

// LoggingMiddleware — Ghi log mọi dispatch.
type LoggingMiddleware struct {
	Logs []string
}

func (m *LoggingMiddleware) Invoke(ctx *core.ModuleContext, next func()) {
	log := fmt.Sprintf("[LOG] %s → %s.%s(%s)",
		time.Now().Format("15:04:05.000"),
		ctx.ModuleName, ctx.Command, fmt.Sprint(ctx.Args))
	next()
	log += fmt.Sprintf(" → %s", ctx.Result)
	m.Logs = append(m.Logs, log)
}

// TimingMiddleware — Đo thời gian dispatch.
type TimingMiddleware struct{}

func (m *TimingMiddleware) Invoke(ctx *core.ModuleContext, next func()) {
	start := time.Now()
	next()
	ctx.Items["elapsed_ms"] = float64(time.Since(start).Microseconds()) / 1000.0
}

// ErrorHandlingMiddleware — Bắt panic, trả error message.
type ErrorHandlingMiddleware struct{}

func (m *ErrorHandlingMiddleware) Invoke(ctx *core.ModuleContext, next func()) {
	defer func() {
		if r := recover(); r != nil {
			ctx.Result = fmt.Sprintf("❌ Error: %v", r)
		}
	}()
	next()
}
