﻿using Lifestream.Schedulers;

namespace Lifestream.Tasks.CrossDC;

internal static class TaskChangeDatacenter
{
    internal static void Enqueue(string destination, string charaName, uint charaWorld)
    {
        var dc = Utils.GetDataCenter(destination);
        PluginLog.Debug($"Beginning data center changing process. Destination: {dc}, {destination}");
        P.TaskManager.Enqueue(() => DCChange.OpenContextMenuForChara(charaName, charaWorld), 5.Minutes(), nameof(DCChange.OpenContextMenuForChara));
        P.TaskManager.Enqueue(DCChange.SelectVisitAnotherDC);
        P.TaskManager.Enqueue(DCChange.ConfirmDcVisitIntention);
        P.TaskManager.Enqueue(() => DCChange.SelectTargetDataCenter(dc), 2.Minutes(), nameof(DCChange.SelectTargetDataCenter));
        P.TaskManager.Enqueue(() => DCChange.SelectTargetWorld(destination), 2.Minutes(), nameof(DCChange.SelectTargetWorld));
        P.TaskManager.Enqueue(DCChange.ConfirmDcVisit, 2.Minutes());
        P.TaskManager.Enqueue(DCChange.ConfirmDcVisit2, 2.Minutes());
        P.TaskManager.Enqueue(DCChange.SelectOk, int.MaxValue);
        P.TaskManager.Enqueue(() => DCChange.SelectServiceAccount(Utils.GetServiceAccount(charaName, charaWorld)), 1.Minutes(), $"SelectServiceAccount_{charaName}@{charaWorld}");
    }
}
