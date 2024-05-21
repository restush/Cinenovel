using Naninovel;

namespace AmoyFeels.Cinenovel
{
    [CommandAlias("cine")]
    public class CinenovelCommand : Command, Command.IPreloadable
    {
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter pathID;
        public DecimalParameter startTime = 0;
        public StringListParameter allowedSampler;

        public override UniTask ExecuteAsync(AsyncToken asyncToken = default)
        {
            CinenovelManager cinenovelManager = Engine.GetService<CinenovelManager>();

            if (!Assigned(Wait))
                Wait = cinenovelManager.Configuration.ForceWait;
            

            return cinenovelManager.PlayCineAsync(pathID, startTime, allowedSampler, asyncToken);

        }

        public UniTask PreloadResourcesAsync()
        {
            return Engine.GetService<CinenovelManager>().Loader.LoadAndHoldAsync(pathID, this);
        }

        public void ReleasePreloadedResources()
        {
            Engine.GetService<CinenovelManager>().Loader.Release(pathID, this);
        }
    }

}