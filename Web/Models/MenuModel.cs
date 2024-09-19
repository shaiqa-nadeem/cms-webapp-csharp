﻿using mojoPortal.Web.UI;

namespace mojoPortal.Web.Models;

public class MenuModel
{
	public MenuModel()
	{ }
	public int Id { get; set; }
	public MenuList Menu { get; set; }
	public mojoMenuItem StartingPage { get; set; }
	public mojoMenuItem CurrentPage { get; set; }
	public bool ShowStartingNode { get; set; }
	public int MaxDepth { get; set; }
}