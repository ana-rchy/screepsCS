// bootstraps shit

using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet {
    public partial class Program {
        static IGame? _game;

        public static void Main() {}

        [JSExport]
        internal static void Init() {
            _game = new Native.World.NativeGame();
            ScreepsMachine.Init(_game);
        }

        [JSExport]
        internal static void Loop() {
            if (_game == null) {
                return;
            }

            _game.Tick();
            ScreepsMachine.Loop();
        }
    }
}