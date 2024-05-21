using Naninovel;

namespace AmoyFeels.Cinenovel
{
    [CommandAlias("stopCine")]
    public class StopCinenovelCommand : Command
    {
        [RequiredParameter, ParameterAlias(NamelessParameterAlias)]
        [Documentation("Name of Cinenovel.")]
        public StringParameter pathID;
        [Documentation("If true, will be unload resource.")]
        public BooleanParameter remove;

        public override UniTask ExecuteAsync(AsyncToken asyncToken = default)
        {
            Engine.GetService<CinenovelManager>().StopCine(pathID,remove);

            return UniTask.CompletedTask;
        }
    }

}