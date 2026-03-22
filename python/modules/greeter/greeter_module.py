"""
Greeter Module — Module thứ 2 trong vũ trụ.
Chứng minh thêm module mới = thêm 1 file + 1 dòng register.
"""
from core.module import IModule


class GreeterModule(IModule):

    @property
    def name(self) -> str:
        return "greeter"

    @property
    def description(self) -> str:
        return "Greeting messages (hello, goodbye)"

    @property
    def commands(self) -> list[str]:
        return ["hello", "goodbye"]

    def execute(self, command: str, args: list[str]) -> str:
        name = " ".join(args) if args else "World"

        match command.lower():
            case "hello":   return f"👋 Hello, {name}! Welcome to the Universe!"
            case "goodbye": return f"🌙 Goodbye, {name}! See you among the stars!"
            case _:         return f"Unknown command '{command}'. Available: {', '.join(self.commands)}"
