package core

// ModuleLifecycle — optional lifecycle hooks cho modules.
// Module implement interface này sẽ được gọi hooks tự động.
type ModuleLifecycle interface {
	OnInitializing() error
	OnInitialized() error
	OnShuttingDown() error
	OnShutdown() error
}
