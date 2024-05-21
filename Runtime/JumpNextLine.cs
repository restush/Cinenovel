using Naninovel;
using Naninovel.Commands;
using UnityEngine;

namespace AmoyFeels.Cinenovel
{
    /// <summary>
    /// This is for resume played script after using Cinenovel aka Timeline.
    /// </summary>
    public class JumpNextLine : MonoBehaviour
    {
        [SerializeField] private bool ContinueOnNextLine = true;

        [ContextMenu(nameof(Jump))]
        public void Jump()
        {
            if (!Engine.Initialized)
            {
                Debug.LogWarning("Nanovel is not initialized");
                return;
            }
            IScriptPlayer scriptPlayer = Engine.GetService<IScriptPlayer>();
            var nextCMD = scriptPlayer.Playlist.GetCommandAfterLine(scriptPlayer.PlayedIndex + 1, 0);

            if (nextCMD is Stop stop && stop != null)
            {
                scriptPlayer.Resume(nextCMD.PlaybackSpot.LineIndex);
                if (ContinueOnNextLine)
                    Engine.GetService<IInputManager>().GetContinue().Activate(1);

            }
            else
            {
                scriptPlayer.Resume(scriptPlayer.PlayedIndex + 1);
                if (ContinueOnNextLine)
                    Engine.GetService<IInputManager>().GetContinue().Activate(1);
            }
        }
    }

}