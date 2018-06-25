using System;

namespace CommonHelpers.Gardens
{
    public interface IGardenService
    {
        void SubscribeOnStart(Action onstartAction);

        void SubscribeOnStop(Action onStopAction);

        void ForceStop();
        void ForceStart();
    }
}