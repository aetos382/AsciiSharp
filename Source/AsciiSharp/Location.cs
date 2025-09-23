using System;

namespace AsciiSharp;

public readonly record struct Position(int Line, int Column);

public readonly record struct Location(Position Start, Position End);