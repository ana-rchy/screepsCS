// bootstraps shit

using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet {
    public partial class Program {
        static IGame? _game;
        static ScreepsMachine? _screepsMachine;

        public static void Main() {}

        [JSExport]
        internal static void Init() {
            _game = new Native.World.NativeGame();
            _screepsMachine = new ScreepsMachine(_game);
        }

        [JSExport]
        internal static void Loop() {
            if (_game == null || _screepsMachine == null) {
                return;
            }

            _game.Tick();
            _screepsMachine.Loop();
        }
    }
}