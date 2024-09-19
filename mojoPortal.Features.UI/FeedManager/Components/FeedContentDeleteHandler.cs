﻿using System;
using mojoPortal.Business;
using mojoPortal.Business.WebHelpers;

namespace mojoPortal.Features;

public class FeedContentDeleteHandler : ContentDeleteHandlerProvider
{
	public FeedContentDeleteHandler() { }

	public override void DeleteContent(int moduleId, Guid moduleGuid) => RssFeed.DeleteByModule(moduleId);
}
