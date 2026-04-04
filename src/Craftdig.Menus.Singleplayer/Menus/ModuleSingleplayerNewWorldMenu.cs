namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerNewWorldMenu(
    RootText text,
    AppStyle s,
    ModuleEnts ents,
    ModuleSingleplayerCreateWorldAction singleplayerCreateWorldAction,
    ModuleSingleplayerLoadWorldAction singleplayerLoadWorldAction)
{
    public void Create(EntMut root) => Create(root, null);

    public void Create(EntMut root, WorldMeta? template)
    {
        var gameModes = ents.Set.Where(x => x.IsGameMode).OrderBy(x => x.Order).ToList();
        var difficulties = ents.Set.Where(x => x.IsDifficulty).OrderBy(x => x.Order).ToList();

        string defaultName = "New World";
        var name = new StringBuilder(template?.Name ?? defaultName);
        var seed = new StringBuilder(template != null ? template.Seed.ToString() : string.Empty);
        int gameModeIndex = template != null ? gameModes.IndexOf(template.GameMode) : 0;
        int difficultyIndex = template != null ? difficulties.IndexOf(template.Difficulty) : 0;

        Node(root, out var form)
            .Mutate(s.VerticalList)
            .OffsetV((0, s.ItemHeight))
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal);
        {
            Node(form)
                .Mutate(s.Label)
                .TextV("World Name");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(29)
                .StringBuilderV(name)
                .IsInitialFocusV(true);

            Node(form)
                .Mutate(s.Label)
                .TextV("Seed");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(29)
                .StringBuilderV(seed);

            Node(form)
                .Mutate(s.Button)
                .OnPressF(() => gameModeIndex = (gameModeIndex + 1) % gameModes.Count)
                .TextF(() => text.Format("Game Mode: {0}", gameModes[gameModeIndex].Name));

            Node(form)
                .Mutate(s.Button)
                .OnPressF(() =>
                {
                    if (gameModes[gameModeIndex].LockedDifficulty == default)
                        difficultyIndex = (difficultyIndex + 1) % difficulties.Count;
                })
                .TextF(() => text.Format("Difficulty: {0}", gameModes[gameModeIndex].LockedDifficulty == default ?
                    difficulties[difficultyIndex].Name :
                    gameModes[gameModeIndex].LockedDifficulty.Name))
                .IsInputDisabledF(() => gameModes[gameModeIndex].LockedDifficulty != default);
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight - s.ItemHeight))
            .AlignmentV(Alignment.Horizontal | Alignment.Bottom)
            .ColorV(s.BoardColor);
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.HorizontalList)
                .AlignmentV(Alignment.Center)
                .OffsetMultiplierV(s.ItemSpacingXS)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingL)
                .ColorV(s.BoardColor2);
            {
                Node(buttonsList, out var leftButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(leftButtonsVertical)
                        .OnPressF(() =>
                        {
                            string worldName = name.ToString();
                            string worldSeed = seed.ToString();
                            var gameMode = gameModes[gameModeIndex];
                            var difficulty = difficulties[difficultyIndex];

                            if (string.IsNullOrEmpty(worldName))
                                worldName = defaultName;

                            if (gameMode.LockedDifficulty != default)
                                difficulty = gameMode.LockedDifficulty;

                            if (!int.TryParse(worldSeed, out int numberSeed))
                            {
                                if (string.IsNullOrEmpty(worldSeed))
                                    numberSeed = new Random().Next();
                                else numberSeed = worldSeed.GetHashCode();
                            }

                            var paths = singleplayerCreateWorldAction.Run(new(worldName, numberSeed, gameMode, difficulty));
                            singleplayerLoadWorldAction.Run(paths);
                        })
                        .TextV("Create New World")
                        .Mutate(s.Button);
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(() => root.StackRootV.NodeStack.Pop())
                        .TextV("Cancel")
                        .Mutate(s.Button);
                }
            }
        }
    }
}
