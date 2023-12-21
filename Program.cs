using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet {
    public static partial class Program {
        private static IGame? game;

        public static void Main() {}

        [JSExport]
        internal static void Init() {
            game = new Native.World.NativeGame();
        }

        [JSExport]
        internal static void Loop() {
            if (game == null) { return; }

            game.Tick();
            Console.WriteLine($"Hello world from C#, the current tick is {game.Time}");
        }
    }
}