namespace Zenvin.Settings.Samples {
	public struct Keybinding {
		public string Id { get; set; }
		public string Path { get; set; }


		public override string ToString () {
			var pathEmpty = string.IsNullOrWhiteSpace (Path);
			var idEmpty = string.IsNullOrWhiteSpace (Id);

			if (pathEmpty && idEmpty)
				return "Keybinding [empty]";
			if (!pathEmpty && !idEmpty)
				return $"Keybinding [{Path}] (overrides {Id})";
			if (idEmpty)
				return $"Keybinding [{Path}]";

			return $"Keybinding [empty] overrides Binding {Id}";
		}
	}
}
