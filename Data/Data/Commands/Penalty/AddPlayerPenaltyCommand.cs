﻿using Data.Enums;
using MediatR;

namespace Data.Commands.Penalty;

public class AddPlayerPenaltyCommand : IRequest<(ControllerEnums.ReturnState, Guid?)>
{
    public PenaltyType PenaltyType { get; set; }
    public PenaltyScope PenaltyScope { get; set; }
    public string Reason { get; set; }
    public bool Automated{ get; set; }
    public TimeSpan Duration { get; set; }
    public string AdminIdentity { get; set; }
    public string TargetIdentity { get; set; }
    public Guid InstanceGuid { get; set; }
    public Guid InstanceApiKey { get; set; }

}
