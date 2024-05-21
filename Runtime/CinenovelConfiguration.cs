using Naninovel;
using UnityEngine;

namespace AmoyFeels.Cinenovel
{
    [EditInProjectSettings]
    public class CinenovelConfiguration : Configuration
    {
        public const string DefaultPathPrefix = "Cinenovel";
        public ResourceLoaderConfiguration Loader = new ResourceLoaderConfiguration { PathPrefix = DefaultPathPrefix };

        [Tooltip("If true, the Cinenovel will not proceed to the next command until it has finished playing. \n\nDefault true." +
            "\n\nDoes not override Command.Wait when assigned.")]
        public bool ForceWait = true;

        [Tooltip("Skip speed when playing Cinenovel.\n\nDefault 1.25 skip speed.")]
        [Range(1f, 10)]
        public float SkipSpeed = 1.25f;

        [Tooltip("Allowed inputs that can be used while playing a Cinenovel based on 'Bindings' in Input Configuration.")]
        public string[] allowedInputSamplers = new string[] { "Skip", "Rollback" };
    }

}