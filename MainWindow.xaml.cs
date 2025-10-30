using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace EmojiViewer;

public partial class MainWindow : Window
{
    private Dictionary<string, Dictionary<string, List<string>>> emojiCategories;
    private List<string> recentEmojis;
    private HashSet<string> favoriteEmojis;
    private const string RecentEmojisFile = "recent_emojis.json";
    private const string FavoriteEmojisFile = "favorite_emojis.json";
    private const int MaxRecentEmojis = 80;

    private TabControl mainTabControl;

    public MainWindow()
    {
        InitializeComponent();
        LoadRecentEmojis();
        LoadFavoriteEmojis();
        InitializeEmojiCategories();
        //CreateTabs();

        // Root layout
        // Root layout
        var root = new DockPanel
        {
            LastChildFill = true,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
        };

        // === Top button row with title bar ===
        var titleBar = new DockPanel
        {
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
            LastChildFill = true  // Add this to fill remaining space with dragArea
        };

        // Window buttons (right side) - ADD FIRST
        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var btnMin = CreateWindowButton("─");
        btnMin.Click += (s, e) => this.WindowState = WindowState.Minimized;

        var btnMax = CreateWindowButton("☐");
        btnMax.Click += (s, e) =>
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        };

        var btnClose = CreateWindowButton("X", Brushes.Red);
        btnClose.Click += (s, e) => this.Close();

        buttonRow.Children.Add(btnMin);
        buttonRow.Children.Add(btnMax);
        buttonRow.Children.Add(btnClose);

        DockPanel.SetDock(buttonRow, Dock.Right);  // Dock to right
        titleBar.Children.Add(buttonRow);  // Add buttons first

        // Draggable area (fills remaining space) - ADD SECOND
        var dragArea = new Border
        {
            Background = Brushes.Transparent,
            Cursor = Cursors.SizeAll
        };

        dragArea.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 1)
            {
                this.DragMove();
            }
            else if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        };

        titleBar.Children.Add(dragArea);  // Add dragArea last (will fill remaining space)

        DockPanel.SetDock(titleBar, Dock.Top);
        root.Children.Add(titleBar);

        // === Tabs ===
        mainTabControl = CreateTabs();
        root.Children.Add(mainTabControl);

        this.Content = root;
    }

    private Button CreateWindowButton(string text, Brush? foreground = null)
    {
        return new Button
        {
            Content = new TextBlock
            {
                Text = text,
                Foreground = foreground ?? Brushes.White,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            },
            Width = 40,
            Height = 30,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Cursor = Cursors.Hand
        };
    }


    private void LoadRecentEmojis()
    {
        try
        {
            if (File.Exists(RecentEmojisFile))
            {
                string json = File.ReadAllText(RecentEmojisFile);
                recentEmojis = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            else
            {
                recentEmojis = new List<string>();
            }
        }
        catch
        {
            recentEmojis = new List<string>();
        }
    }

    private void SaveRecentEmojis()
    {
        try
        {
            string json = JsonConvert.SerializeObject(recentEmojis, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(RecentEmojisFile, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save recent emojis: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LoadFavoriteEmojis()
    {
        try
        {
            if (File.Exists(FavoriteEmojisFile))
            {
                string json = File.ReadAllText(FavoriteEmojisFile);
                var favoritesList = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                favoriteEmojis = new HashSet<string>(favoritesList);
            }
            else
            {
                favoriteEmojis = new HashSet<string>();
            }
        }
        catch
        {
            favoriteEmojis = new HashSet<string>();
        }
    }

    private void SaveFavoriteEmojis()
    {
        try
        {
            var favoritesList = favoriteEmojis.ToList();
            string json = JsonConvert.SerializeObject(favoritesList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FavoriteEmojisFile, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save favorite emojis: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddToRecentEmojis(string emoji)
    {
        // Remove if already exists to avoid duplicates
        recentEmojis.Remove(emoji);

        // Add to beginning of list
        recentEmojis.Insert(0, emoji);

        // Keep only the most recent emojis
        if (recentEmojis.Count > MaxRecentEmojis)
        {
            recentEmojis.RemoveRange(MaxRecentEmojis, recentEmojis.Count - MaxRecentEmojis);
        }

        SaveRecentEmojis();
    }

    private void ToggleFavorite(string emoji)
    {
        if (favoriteEmojis.Contains(emoji))
        {
            favoriteEmojis.Remove(emoji);
        }
        else
        {
            favoriteEmojis.Add(emoji);
        }

        SaveFavoriteEmojis();
    }

    private void InitializeEmojiCategories()
    {
        emojiCategories = new Dictionary<string, Dictionary<string, List<string>>>
        {
            ["Faces"] = new Dictionary<string, List<string>>
            {
                ["Basic Faces"] = new List<string>
                {
                    "😀", "😃", "😄", "😁", "😆", "😅", "🤣", "😂", "🙂", "🙃",
                    "😉", "😊", "😇", "🥰", "😍", "🤩", "😘", "😗", "😚", "😙",
                    "😋", "😛", "😜", "🤪", "😝", "🤑", "🤗", "🤭", "🤫", "🤔",
                    "🤐", "🤨", "😐", "😑", "😶", "😏", "😒", "🙄", "😬", "🤥",
                    "😔", "😪", "🤤", "😴", "😷", "🤒", "🤕", "🤢", "🤮", "🤧",
                    "🥵", "🥶", "🥴", "😵", "🤯", "🤠", "🥳", "😎", "🤓", "🧐",
                    "😕", "😟", "🙁", "☹️", "😮", "😯", "😲", "😳", "🥺", "😦",
                    "😧", "😨", "😰", "😥", "😢", "😭", "😱", "😖", "😣", "😞"
                },
                ["Cat Faces"] = new List<string>
                {
                    "😸", "😹", "😺", "😻", "😼", "😽", "🙀", "😿", "😾"
                },
                ["Monkey Faces"] = new List<string>
                {
                    "🙈", "🙉", "🙊", "🐵"
                },
                ["Creature Faces"] = new List<string>
                {
                    "👹", "👺", "🤡", "💩", "👻", "💀", "☠️", "👽", "👾", "🤖",
                    "🎃", "😈", "👿", "🔥", "💫", "⭐", "🌟", "✨", "💥", "💢",
                    "👹", "👺", "🤡", "🦄", "🐲", "🐉", "🦖", "🦕"
                }
            },

            ["People"] = new Dictionary<string, List<string>>
            {
                ["Basic People"] = new List<string>
                {
                    "👶", "🧒", "👦", "👧", "🧑", "👱", "👨", "🧔", "👨‍🦰", "👨‍🦱",
                    "👨‍🦳", "👨‍🦲", "👩", "👩‍🦰", "👩‍🦱", "👩‍🦳", "👩‍🦲", "👱‍♀️", "👱‍♂️", "🧓",
                    "👴", "👵"
                },
                ["Role and Activities"] = new List<string>
                {
                    "👨‍⚕️", "👩‍⚕️", "👨‍🌾", "👩‍🌾", "👨‍🍳", "👩‍🍳", "👨‍🎓", "👩‍🎓", "👨‍🎤", "👩‍🎤",
                    "👨‍🏫", "👩‍🏫", "👨‍🏭", "👩‍🏭", "👨‍💻", "👩‍💻", "👨‍💼", "👩‍💼", "👨‍🔧", "👩‍🔧",
                    "👨‍🔬", "👩‍🔬", "👨‍🎨", "👩‍🎨", "👨‍🚒", "👩‍🚒", "👨‍✈️", "👩‍✈️", "👨‍🚀", "👩‍🚀",
                    "👨‍⚖️", "👩‍⚖️", "👰", "🤵", "👸", "🤴", "🦸", "🦹", "🧙", "🧚",
                    "🧛", "🧜", "🧝", "🧞", "🧟", "💆", "💇", "🚶", "🏃", "💃"
                },
                ["Body Gestures"] = new List<string>
                {
                    "🙍", "🙍‍♂️", "🙍‍♀️", "🙎", "🙎‍♂️", "🙎‍♀️", "🙅", "🙅‍♂️", "🙅‍♀️", "🙆",
                    "🙆‍♂️", "🙆‍♀️", "💁", "💁‍♂️", "💁‍♀️", "🙋", "🙋‍♂️", "🙋‍♀️", "🧏", "🧏‍♂️",
                    "🧏‍♀️", "🙇", "🙇‍♂️", "🙇‍♀️", "🤦", "🤦‍♂️", "🤦‍♀️", "🤷", "🤷‍♂️", "🤷‍♀️"
                },
                ["Comic Style"] = new List<string>
                {
                    "🦹", "🦸", "🧙", "🧚", "🧛", "🧜", "🧝", "🧞", "🧟", "🎅",
                    "🤶", "🧙‍♀️", "🧙‍♂️", "🧚‍♀️", "🧚‍♂️", "🧛‍♀️", "🧛‍♂️", "🧜‍♀️", "🧜‍♂️", "🧝‍♀️"
                },
                ["Body and Fashion"] = new List<string>
                {
                    "👗", "👚", "👕", "👖", "👔", "🧥", "🥼", "🦺", "👘", "🥻",
                    "🩱", "🩲", "🩳", "👙", "👠", "👡", "👢", "👞", "👟", "🥾",
                    "🥿", "👒", "🎩", "🎓", "👑", "⛑️", "📿", "💄", "💍", "💎"
                },
                ["Hand Gesture"] = new List<string>
                {
                    "👋", "🤚", "🖐️", "✋", "🖖", "👌", "🤌", "🤏", "✌️", "🤞",
                    "🤟", "🤘", "🤙", "👈", "👉", "👆", "🖕", "👇", "☝️", "👍",
                    "👎", "👊", "✊", "🤛", "🤜", "👏", "🙌", "👐", "🤲", "🤝"
                },
                ["Love"] = new List<string>
                {
                    "💋", "💌", "💘", "💝", "💖", "💗", "💓", "💞", "💕", "💟",
                    "❣️", "💔", "❤️‍🔥", "❤️‍🩹", "❤️", "🧡", "💛", "💚", "💙", "💜",
                    "🤎", "🖤", "🤍", "💯", "💢", "💥", "💫", "💦", "💨", "🕳️"
                },
                ["Couple and Family"] = new List<string>
                {
                    "👫", "👬", "👭", "👪", "👨‍👩‍👧", "👨‍👩‍👧‍👦", "👨‍👩‍👦‍👦", "👨‍👩‍👧‍👧", "👨‍👨‍👦", "👨‍👨‍👧",
                    "👨‍👨‍👧‍👦", "👨‍👨‍👦‍👦", "👨‍👨‍👧‍👧", "👩‍👩‍👦", "👩‍👩‍👧", "👩‍👩‍👧‍👦", "👩‍👩‍👦‍👦", "👩‍👩‍👧‍👧", "👨‍👦", "👨‍👦‍👦",
                    "👨‍👧", "👨‍👧‍👦", "👨‍👧‍👧", "👩‍👦", "👩‍👦‍👦", "👩‍👧", "👩‍👧‍👦", "👩‍👧‍👧"
                }
            },

            ["Leisure"] = new Dictionary<string, List<string>>
            {
                ["Celebration"] = new List<string>
                {
                    "🎉", "🎊", "🥳", "🎈", "🎁", "🎀", "🎂", "🍰", "🧁", "🍾",
                    "🥂", "🍻", "🎆", "🎇", "✨", "🎃", "🎄", "🎋", "🎍", "🎑"
                },
                ["Entertainment"] = new List<string>
                {
                    "🎪", "🎭", "🩰", "🎨", "🎬", "🎤", "🎧", "🎼", "🎹", "🥁",
                    "🎷", "🎺", "🎸", "🪕", "🎻", "🎲", "♠️", "♥️", "♦️", "♣️",
                    "🃏", "🀄", "🎯", "🎳", "🎮", "🕹️", "🎰", "🧩"
                },
                ["Sport"] = new List<string>
                {
                    "⚽", "🏀", "🏈", "⚾", "🥎", "🎾", "🏐", "🏉", "🥏", "🎱",
                    "🪀", "🏓", "🏸", "🏒", "🏑", "🥍", "🏏", "🪃", "🥅", "⛳",
                    "🏹", "🎣", "🤿", "🥊", "🥋", "🎽", "🛹", "🛷", "⛸️", "🥌"
                },
                ["Music"] = new List<string>
                {
                    "🎵", "🎶", "🎼", "🎹", "🥁", "🎷", "🎺", "🎸", "🪕", "🎻",
                    "🪗", "🎤", "🎧", "📻", "📀", "💿", "💾", "💽", "🎙️", "🎚️"
                },
                ["Cards and Chess"] = new List<string>
                {
                    "♠️", "♥️", "♦️", "♣️", "🃏", "🀄", "♟️", "♜", "♝", "♛",
                    "♚", "♞", "♜", "♟", "♙", "♖", "♕", "♔", "♗", "♘"
                },
                ["Japanese Chess"] = new List<string>
                {
                    "🀀", "🀁", "🀂", "🀃", "🀄", "🀅", "🀆", "🀇", "🀈", "🀉",
                    "🀊", "🀋", "🀌", "🀍", "🀎", "🀏"
                },
                ["Draughts and Checkers"] = new List<string>
                {
                    "⚫", "⚪", "🔴", "🟤", "🟡", "🟢", "🔵", "🟣", "🟠", "⭕"
                },
                ["Go Markers"] = new List<string>
                {
                    "⚫", "⚪", "🔘", "⭕", "🚫", "💯", "💮", "🔴", "🔵", "🟢"
                }
            },

            ["Nature (Animals)"] = new Dictionary<string, List<string>>
            {
                ["Animals"] = new List<string>
                {
                    "🐶", "🐱", "🐭", "🐹", "🐰", "🦊", "🐻", "🐼", "🐨", "🐯",
                    "🦁", "🐮", "🐷", "🐽", "🐸", "🐵", "🐒", "🐔", "🐧", "🐦",
                    "🐤", "🐣", "🐥", "🦆", "🦅", "🦉", "🦇", "🐺", "🐗", "🐴",
                    "🦄", "🐝", "🐛", "🦋", "🐌", "🐞", "🐜", "🦟", "🦗", "🕷️",
                    "🦂", "🐢", "🐍", "🦎", "🦖", "🦕", "🐙", "🦑", "🦐", "🦞",
                    "🦀", "🐡", "🐠", "🐟", "🐬", "🐳", "🐋", "🦈", "🐊", "🐅",
                    "🐆", "🦓", "🦍", "🦧", "🐘", "🦛", "🦏", "🐪", "🐫", "🦒"
                },
                ["Ninja Cat"] = new List<string>
                {
                    "🥷", "🐱‍👤", "🐱‍🏍", "🐱‍💻", "🐱‍🐉", "🐱‍👓", "🐱‍🚀"
                },
                ["Environment/Weather"] = new List<string>
                {
                    "🌞", "🌝", "🌛", "🌜", "🌚", "🌕", "🌖", "🌗", "🌘", "🌑",
                    "🌒", "🌓", "🌔", "🌙", "🌎", "🌍", "🌏", "💫", "⭐", "🌟",
                    "✨", "⚡", "☄️", "💥", "🔥", "🌪️", "🌈", "☀️", "🌤️", "⛅",
                    "🌦️", "🌧️", "⛈️", "🌩️", "🌨️", "❄️", "☃️", "⛄", "🌬️", "💨",
                    "🌊", "💧", "💦", "🌿", "🍃", "🌱", "🌳", "🌲", "🎋", "🎍"
                },
                ["Time"] = new List<string>
                {
                    "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙",
                    "🕚", "🕛", "🕜", "🕝", "🕞", "🕟", "🕠", "🕡", "🕢", "🕣",
                    "🕤", "🕥", "🕦", "🕧", "⏰", "⏲️", "⏱️", "⏳", "⌛", "📅",
                    "📆", "🗓️", "📋", "📌", "📍", "📎", "🔗", "📏", "📐"
                }
            },

            ["Food/Drinks"] = new Dictionary<string, List<string>>
            {
                ["Fruit and Vegetables"] = new List<string>
                {
                    "🍎", "🍊", "🍋", "🍌", "🍉", "🍇", "🍓", "🫐", "🍈", "🍒",
                    "🍑", "🥭", "🍍", "🥥", "🥝", "🍅", "🍆", "🥑", "🥦", "🥬",
                    "🥒", "🌶️", "🫑", "🌽", "🥕", "🫒", "🧄", "🧅", "🥔", "🍠",
                    "🥐", "🍞", "🥖", "🥨", "🧀", "🥚", "🍳", "🧈", "🥞", "🧇"
                },
                ["Drinks"] = new List<string>
                {
                    "☕", "🍵", "🧃", "🥤", "🍶", "🍺", "🍻", "🥂", "🍷", "🥃",
                    "🍸", "🍹", "🧉", "🍾", "🧊", "🥛", "🍼", "🫖", "🧋", "🥤"
                },
                ["Dishware"] = new List<string>
                {
                    "🍽️", "🍴", "🥄", "🔪", "🥢", "🍷", "🍸", "🍹", "🍺", "🍻",
                    "🥂", "🥃", "🫖", "☕", "🍵", "🧃", "🥛", "🧊", "🥤", "🍾"
                }
            },

            ["City"] = new Dictionary<string, List<string>>
            {
                ["Living in a City"] = new List<string>
                {
                    "🏠", "🏡", "🏘️", "🏚️", "🏗️", "🏭", "🏢", "🏬", "🏣", "🏤",
                    "🏥", "🏦", "🏨", "🏪", "🏫", "🏩", "💒", "🏛️", "⛪", "🕌",
                    "🛕", "🕍", "⛩️", "🕋", "⛲", "⛱️", "🏖️", "🏝️"
                },
                ["Locations and Landmarks"] = new List<string>
                {
                    "🗼", "🗽", "⛪", "🕌", "🛕", "🕍", "⛩️", "🕋", "⛲", "⛱️",
                    "🏖️", "🏝️", "🏜️", "🌋", "⛰️", "🏔️", "🗻", "🏕️", "⛺", "🏞️",
                    "🛣️", "🛤️", "🌉", "🌁", "🏙️", "🌆", "🌇", "🌃", "🌌", "🎡"
                },
                ["Transports"] = new List<string>
                {
                    "🚗", "🚕", "🚙", "🚌", "🚎", "🏎️", "🚓", "🚑", "🚒", "🚐",
                    "🚚", "🚛", "🚜", "🏍️", "🛵", "🚲", "🛴", "🚁", "✈️", "🛩️",
                    "🚀", "🛸", "🚢", "⛵", "🚤", "⛴️", "🚆", "🚄", "🚅", "🚈",
                    "🚝", "🚞", "🚋", "🚃", "🚇", "🚊", "🚉", "🚂", "🚆", "🚄"
                },
                ["Sights"] = new List<string>
                {
                    "🎪", "🎡", "🎢", "🎠", "⛲", "🏖️", "🏝️", "🏔️", "🗻", "🏕️",
                    "🏞️", "🌉", "🌁", "🏙️", "🌆", "🌇", "🌃", "🌌", "🎆", "🎇"
                }
            },

            ["Office"] = new Dictionary<string, List<string>>
            {
                ["Money"] = new List<string>
                {
                    "💰", "💴", "💵", "💶", "💷", "💸", "💳", "🧾", "💎", "⚖️",
                    "🏧", "💹", "💱", "💲", "🪙", "💰", "🪪", "🏷️", "📊", "📈"
                },
                ["Work"] = new List<string>
                {
                    "💼", "👔", "📊", "📈", "📉", "📋", "📌", "📍", "📎", "🖇️",
                    "📏", "📐", "✂️", "🗃️", "🗄️", "🗑️", "🔒", "🔓", "🔏", "🔐",
                    "🔑", "🗝️", "🔨", "🪓", "⛏️", "⚒️", "🛠️", "⚙️", "🔧", "🔩"
                },
                ["Communications"] = new List<string>
                {
                    "📝", "✏️", "🖊️", "🖋️", "✒️", "🖌️", "🖍️", "📚", "📖", "📗",
                    "📘", "📙", "📓", "📔", "📒", "📃", "📄", "📰", "🗞️", "📑",
                    "🔖", "🏷️", "📞", "☎️", "📟", "📠", "📧", "📨", "📩", "📪"
                },
                ["Grading"] = new List<string>
                {
                    "💯", "💮", "🏆", "🥇", "🥈", "🥉", "🏅", "🎖️", "🏆", "📜",
                    "🎓", "📚", "📖", "📝", "✏️", "📐", "📏", "🖊️", "🖋️", "✒️"
                }
            },

            ["IT/UI"] = new Dictionary<string, List<string>>
            {
                ["Devices"] = new List<string>
                {
                    "💻", "🖥️", "🖨️", "⌨️", "🖱️", "🖲️", "💽", "💾", "💿", "📀",
                    "🧮", "📱", "📞", "☎️", "📟", "📠", "📺", "📻", "🎙️", "🎚️",
                    "🎛️", "🧭", "⏱️", "⏲️", "⏰", "🕰️", "⌛", "⏳", "📡", "🔋"
                },
                ["User Interface"] = new List<string>
                {
                    "🔄", "🔃", "🔁", "🔂", "▶️", "⏸️", "⏯️", "⏹️", "⏺️", "⏭️",
                    "⏮️", "⏪", "⏩", "⏫", "⏬", "◀️", "🔼", "🔽", "➡️", "⬅️",
                    "⬆️", "⬇️", "↗️", "↘️", "↙️", "↖️", "↕️", "↔️", "↪️", "↩️",
                    "⤴️", "⤵️", "🔀", "🔁", "🔂", "🔄", "🔃", "🎛️", "🎚️", "📶"
                },
                ["Informational Signs"] = new List<string>
                {
                    "⚠️", "🚸", "⛔", "🚫", "🚳", "🚭", "🚯", "🚱", "🚷", "📵",
                    "🔞", "☢️", "☣️", "⬆️", "↗️", "➡️", "↘️", "⬇️", "↙️", "⬅️",
                    "↖️", "↕️", "↔️", "↪️", "↩️", "⤴️", "⤵️", "🔃", "🔄", "🔙",
                    "🔚", "🔛", "🔜", "🔝", "🛂", "🛃", "🛄", "🛅", "⚡", "🔌"
                }
            },

            ["MISC/Squares/Circles"] = new Dictionary<string, List<string>>
            {
                ["All Geometric Emojis"] = new List<string>
                {
                    "⚫", "⚪", "🔴", "🔵", "🟠", "🟡", "🟢", "🟣", "🟤", "🔲",
                    "🔳", "⬛", "⬜", "◼️", "◻️", "◾", "◽", "▪️", "▫️", "🔶",
                    "🔷", "🔸", "🔹", "🔺", "🔻", "💠", "🔘", "🔲", "🔳", "⭐",
                    "🌟", "✨", "⚡", "💥", "💫", "💦", "💨", "🕳️", "💣", "💢",
                    "💤", "💨", "💫", "💥", "💢", "💦", "💤", "🗨️", "💭", "🗯️"
                },
                ["Religious Symbols"] = new List<string>
                {
                    "☪️", "✡️", "🔯", "🕉️", "☸️", "✝️", "☦️", "☪️", "☮️", "🕎",
                    "🔱", "🆔", "⚛️", "🉐", "㊙️", "㊗️", "🈴", "🈵", "🈹", "🈲",
                    "🅰️", "🅱️", "🆎", "🆑", "🅾️", "🆘", "❌", "⭕", "🛑", "⛔"
                }
            }
        };
    }

    private TabItem CreateRecentTab()
    {
        var recentTab = new TabItem
        {
            Header = "Recent",
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70))
        };

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
        };

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            Name = "RecentEmojisPanel"
        };

        RefreshRecentEmojis(wrapPanel);

        scrollViewer.Content = wrapPanel;
        recentTab.Content = scrollViewer;

        return recentTab;
    }

    private TabItem CreateFavoritesTab()
    {
        var favoritesTab = new TabItem
        {
            Header = "Favorites",
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            Foreground = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70))
        };

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
        };

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            Name = "FavoriteEmojisPanel"
        };

        RefreshFavoriteEmojis(wrapPanel);

        scrollViewer.Content = wrapPanel;
        favoritesTab.Content = scrollViewer;

        return favoritesTab;
    }

    private void RefreshRecentEmojis(WrapPanel panel)
    {
        panel.Children.Clear();
        foreach (var emoji in recentEmojis)
        {
            var button = CreateEmojiButton(emoji);
            panel.Children.Add(button);
        }
    }

    private void RefreshFavoriteEmojis(WrapPanel panel)
    {
        panel.Children.Clear();
        foreach (var emoji in favoriteEmojis)
        {
            var button = CreateEmojiButton(emoji);
            panel.Children.Add(button);
        }
    }

    private Button CreateEmojiButton(string emoji)
    {
        var emojiTextBlock = new Emoji.Wpf.TextBlock
        {
            Text = emoji,
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var button = new Button
        {
            Content = emojiTextBlock,
            Width = 90,
            Height = 90,
            Margin = new Thickness(2),
            Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Style = CreateButtonStyle(),
            Tag = emoji
        };

        // Add improved context menu with custom style
        var contextMenu = new ContextMenu
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(4),
            HasDropShadow = true,
            Style = CreateContextMenuStyle()
        };

        var favoriteMenuItem = new MenuItem
        {
            Header = CreateMenuHeader(
                favoriteEmojis.Contains(emoji) ? "Remove from Favorites" : "Add to Favorites",
                favoriteEmojis.Contains(emoji) ? "❤️" : "🤍"
            ),
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            Foreground = Brushes.White,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Icon = CreateCleanEmojiIcon(favoriteEmojis.Contains(emoji) ? "❤️" : "🤍")
        };
        favoriteMenuItem.Click += (s, e) => ToggleFavoriteFromContext(emoji, favoriteMenuItem);

        var copyCodeMenuItem = new MenuItem
        {
            Header = CreateMenuHeader("Copy Unicode", "📋"),
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
            Foreground = Brushes.White,
            Height = 32,
            Padding = new Thickness(8, 4, 8, 4),
            Icon = CreateCleanEmojiIcon("📋")
        };
        copyCodeMenuItem.Click += (s, e) => CopyEmojiCode(emoji);

        // Style for menu item hover
        var menuItemStyle = new Style(typeof(MenuItem));
        var hoverTrigger = new Trigger { Property = MenuItem.IsHighlightedProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(MenuItem.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(60, 60, 60))));
        menuItemStyle.Triggers.Add(hoverTrigger);

        favoriteMenuItem.Style = menuItemStyle;
        copyCodeMenuItem.Style = menuItemStyle;

        contextMenu.Items.Add(favoriteMenuItem);
        contextMenu.Items.Add(copyCodeMenuItem);

        button.ContextMenu = contextMenu;
        button.Click += (sender, e) => CopyToClipboard(emoji);

        return button;
    }

    private Style CreateContextMenuStyle()
    {
        var style = new Style(typeof(ContextMenu));

        // Remove the default icon column background
        var template = new ControlTemplate(typeof(ContextMenu));
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(45, 45, 45)));
        border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(70, 70, 70)));
        border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

        var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
        stackPanel.SetValue(StackPanel.IsItemsHostProperty, true);

        border.AppendChild(stackPanel);
        template.VisualTree = border;

        style.Setters.Add(new Setter(ContextMenu.TemplateProperty, template));

        return style;
    }

    private TextBlock CreateCleanEmojiIcon(string emoji)
    {
        return new TextBlock
        {
            Text = emoji,
            FontSize = 17,
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0)
        };
    }

    private StackPanel CreateMenuHeader(string text, string emoji)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0)
        };

        // Don't add emoji here since it's in the Icon now
        var labelText = new TextBlock
        {
            Text = text,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.White
        };

        panel.Children.Add(labelText);

        return panel;
    }

    private void ToggleFavoriteFromContext(string emoji, MenuItem menuItem)
    {
        ToggleFavorite(emoji);
        menuItem.Header = favoriteEmojis.Contains(emoji) ? "Remove from Favorites" : "Add to Favorites";

        // Refresh favorites tab if it exists
        RefreshAllTabs();
    }

    private void RefreshAllTabs()
    {
        if (mainTabControl == null) return;

        foreach (TabItem tab in mainTabControl.Items)
        {
            var config = tab.Tag as TabConfig;
            if (config == null) continue;

            if (config.Name == "Recent")
            {
                var scrollViewer = tab.Content as ScrollViewer;
                var panel = scrollViewer?.Content as WrapPanel;
                if (panel != null)
                {
                    RefreshRecentEmojis(panel);
                }
            }
            else if (config.Name == "Favorites")
            {
                var scrollViewer = tab.Content as ScrollViewer;
                var panel = scrollViewer?.Content as WrapPanel;
                if (panel != null)
                {
                    RefreshFavoriteEmojis(panel);
                }
            }
        }
    }

    private void CopyEmojiCode(string emoji)
    {
        try
        {
            var codePoints = emoji.EnumerateRunes().Select(r => $"U+{r.Value:X4}");
            var unicodeString = string.Join(" ", codePoints);
            Clipboard.SetText(unicodeString);

            this.Title = $"Emoji Viewer - Copied Unicode: {unicodeString}";

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, e) =>
            {
                this.Title = "Emoji Viewer";
                timer.Stop();
            };
            timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to copy emoji code: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Style CreateButtonStyle()
    {
        var style = new Style(typeof(Button));

        var hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(80, 80, 80))));

        var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
        pressedTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(40, 40, 40))));

        style.Triggers.Add(hoverTrigger);
        style.Triggers.Add(pressedTrigger);

        return style;
    }

    private Image CreateEmojiImage2(string emoji)
    {
        var tb = new TextBlock
        {
            Text = emoji,
            FontFamily = new FontFamily("Segoe UI Emoji"),
            Foreground = Brushes.White,
            FontSize = 16
        };

        var rtb = new RenderTargetBitmap(50, 50, 96, 96, PixelFormats.Pbgra32);
        tb.Measure(new Size(50, 50));
        tb.Arrange(new Rect(new Size(50, 50)));
        rtb.Render(tb);

        return new Image { Source = rtb, Width = 32, Height = 32 };
    }

    private void CopyToClipboard(string emoji)
    {
        try
        {
            // Retry mechanism for clipboard operations
            int maxRetries = 3;
            bool success = false;

            for (int i = 0; i < maxRetries && !success; i++)
            {
                try
                {
                    Clipboard.SetDataObject(emoji, true); // true = keep data after app closes
                    success = true;
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    // Clipboard is busy, wait a bit and retry
                    System.Threading.Thread.Sleep(100);

                    //await Task.Delay(10);
                }
            }

            if (!success)
            {
                throw new Exception("Clipboard is busy");
            }

            AddToRecentEmojis(emoji);
            RefreshAllTabs();

            // Create and show tooltip
            var tooltip = new ToolTip
            {
                Content = "Copied!",
                IsOpen = true,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 4, 8, 4),
                FontSize = 12,
                HasDropShadow = true,
                StaysOpen = false
            };

            // Auto-close tooltip after 1 second
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += (s, e) =>
            {
                tooltip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();

            // Optional: Update window title as backup feedback
            this.Title = $"Emoji Viewer - Copied: {emoji}";
            var titleTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            titleTimer.Tick += (s, e) =>
            {
                this.Title = "Emoji Viewer";
                titleTimer.Stop();
            };
            titleTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to copy emoji: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Now CreateTabs only builds and returns the TabControl
    private TabControl CreateTabs()
    {
        var mainTabControl = new TabControl
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
            BorderThickness = new Thickness(0),
            TabStripPlacement = Dock.Top,
            Style = CreateModernTabControlStyle()
        };

        mainTabControl.SelectionChanged += OnTabSelectionChanged;

        var tabConfigs = new List<TabConfig>
    {
        new TabConfig { Name = "Faces", Emoji = "😀", Category = "Faces" },
        new TabConfig { Name = "People", Emoji = "👤", Category = "People" },
        new TabConfig { Name = "Leisure", Emoji = "🎯", Category = "Leisure" },
        new TabConfig { Name = "Nature", Emoji = "🐾", Category = "Nature (Animals)" },
        new TabConfig { Name = "Food", Emoji = "🍎", Category = "Food/Drinks" },
        new TabConfig { Name = "City", Emoji = "🏙️", Category = "City" },
        new TabConfig { Name = "Office", Emoji = "💼", Category = "Office" },
        new TabConfig { Name = "IT/UI", Emoji = "💻", Category = "IT/UI" },
        new TabConfig { Name = "Misc", Emoji = "🔷", Category = "MISC/Squares/Circles" },
        new TabConfig { Name = "Recent", Emoji = "🕐", Category = null },
        new TabConfig { Name = "Favorites", Emoji = "🧩", Category = null },
    };

        foreach (var config in tabConfigs)
        {
            var tabItem = CreateModernTabItem(config);

            if (config.Category == null)
            {
                if (config.Name == "Recent")
                    tabItem.Content = CreateRecentTabContent();
                else if (config.Name == "Favorites")
                    tabItem.Content = CreateFavoritesTabContent();
            }
            else if (emojiCategories.ContainsKey(config.Category))
            {
                tabItem.Content = CreateCategoryTabContent(emojiCategories[config.Category]);
            }

            mainTabControl.Items.Add(tabItem);
        }

        return mainTabControl;
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var tabControl = sender as TabControl;
        if (tabControl == null) return;

        // Hide text for all unselected tabs
        foreach (TabItem tabItem in tabControl.Items)
        {
            var headerPanel = tabItem.Header as DockPanel;
            if (headerPanel != null && headerPanel.Children.Count > 1)
            {
                var textLabel = headerPanel.Children[0] as TextBlock;
                if (textLabel != null)
                {
                    if (tabItem.IsSelected)
                    {
                        textLabel.Foreground = Brushes.Yellow;
                    }
                    else
                    {
                        textLabel.Foreground = Brushes.White;
                    }
                }
            }
        }
    }

    private TabItem CreateModernTabItem(TabConfig config)
    {
        var tabItem = new TabItem
        {
            Style = CreateModernTabItemStyle(),
            Tag = config
        };

        var headerPanel = new DockPanel
        {
            LastChildFill = true,

        };


        // Emoji icon
        var emojiIcon = new TextBlock
        {
            Text = config.Emoji,
            FontSize = 26,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 5),
            Name = "EmojiIcon"
        };

        DockPanel.SetDock(emojiIcon, Dock.Left);

        // Text label (initially hidden)
        var textLabel = new TextBlock
        {
            Text = config.Name,
            FontSize = 12,
            FontWeight = FontWeights.Medium,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            Margin = new Thickness(8, 0, 0, 0),
            Opacity = 0,
            Name = "TextLabel"
        };

        DockPanel.SetDock(textLabel, Dock.Right);
        headerPanel.Children.Add(emojiIcon);
        headerPanel.Children.Add(textLabel);

        tabItem.Header = headerPanel;

        tabItem.Width = 70; // Initial width for icon only

        return tabItem;
    }

    private void OnTabSelected(object sender, RoutedEventArgs e)
    {
        var tabItem = sender as TabItem;
        var headerPanel = tabItem?.Header as StackPanel;
        if (headerPanel != null)
        {
            var textLabel = headerPanel.Children[1] as TextBlock;
            if (textLabel != null)
            {
                // Animate text appearance
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                textLabel.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
            }
        }
    }

    private void OnTabUnselected(object sender, RoutedEventArgs e)
    {
        var tabItem = sender as TabItem;
        var headerPanel = tabItem?.Header as StackPanel;
        if (headerPanel != null)
        {
            var textLabel = headerPanel.Children[1] as TextBlock;
            if (textLabel != null)
            {
                // Animate text disappearance
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
                textLabel.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
            }
        }
    }

    private Style CreateModernTabControlStyle()
    {
        var style = new Style(typeof(TabControl));

        style.Setters.Add(new Setter(TabControl.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(25, 25, 25))));

        return style;
    }

    private Style CreateModernTabItemStyle()
    {
        var style = new Style(typeof(TabItem));

        // Base setters
        style.Setters.Add(new Setter(TabItem.BackgroundProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(TabItem.BorderThicknessProperty, new Thickness(0)));
        style.Setters.Add(new Setter(TabItem.PaddingProperty, new Thickness(16, 12, 0, 0)));
        style.Setters.Add(new Setter(TabItem.MarginProperty, new Thickness(0)));
        style.Setters.Add(new Setter(TabItem.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(160, 160, 160))));
        style.Setters.Add(new Setter(TabItem.FontSizeProperty, 12.0));
        style.Setters.Add(new Setter(TabItem.CursorProperty, Cursors.Hand));

        // Template for custom appearance
        var template = new ControlTemplate(typeof(TabItem));

        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "Border";
        border.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 3));
        border.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
        border.SetValue(Border.PaddingProperty, new Thickness(16, 12, 0, 0));

        var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.Name = "ContentSite";
        contentPresenter.SetValue(ContentPresenter.ContentSourceProperty, "Header");
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

        border.AppendChild(contentPresenter);
        template.VisualTree = border;

        // Triggers for hover and selection
        var hoverTrigger = new Trigger();
        hoverTrigger.Property = TabItem.IsMouseOverProperty;
        hoverTrigger.Value = true;
        hoverTrigger.Setters.Add(new Setter(TabItem.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(45, 45, 45)), "Border"));

        var selectedTrigger = new Trigger();
        selectedTrigger.Property = TabItem.IsSelectedProperty;
        selectedTrigger.Value = true;
        selectedTrigger.Setters.Add(new Setter(TabItem.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(35, 35, 35)), "Border"));
        selectedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty,
            new SolidColorBrush(Color.FromRgb(0, 120, 215)), "Border"));
        selectedTrigger.Setters.Add(new Setter(TabItem.ForegroundProperty,
            new SolidColorBrush(Color.FromRgb(255, 255, 255))));

        template.Triggers.Add(hoverTrigger);
        template.Triggers.Add(selectedTrigger);

        style.Setters.Add(new Setter(TabItem.TemplateProperty, template));

        return style;
    }

    private ScrollViewer CreateRecentTabContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
        };

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Name = "RecentEmojisPanel"
        };

        RefreshRecentEmojis(wrapPanel);
        scrollViewer.Content = wrapPanel;
        return scrollViewer;
    }

    private ScrollViewer CreateFavoritesTabContent()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
        };

        var wrapPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20),
            Name = "FavoriteEmojisPanel"
        };

        RefreshFavoriteEmojis(wrapPanel);
        scrollViewer.Content = wrapPanel;
        return scrollViewer;
    }

    private ScrollViewer CreateCategoryTabContent(Dictionary<string, List<string>> category)
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = new SolidColorBrush(Color.FromRgb(32, 32, 32))
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(20)
        };

        foreach (var subCategory in category)
        {
            // Add category header with modern styling
            var categoryHeader = new TextBlock
            {
                Text = subCategory.Key,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                Margin = new Thickness(0, 25, 0, 15)
            };

            // Add subtle underline
            var underline = new Rectangle
            {
                Height = 2,
                Fill = new SolidColorBrush(Color.FromRgb(0, 120, 215)),
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 60
            };

            mainPanel.Children.Add(categoryHeader);
            mainPanel.Children.Add(underline);

            var wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 20)
            };

            foreach (var emoji in subCategory.Value)
            {
                var button = CreateEmojiButton(emoji);
                wrapPanel.Children.Add(button);
            }

            mainPanel.Children.Add(wrapPanel);
        }

        scrollViewer.Content = mainPanel;
        return scrollViewer;
    }

    // Helper class for tab configuration
    public class TabConfig
    {
        public string Name { get; set; }
        public string Emoji { get; set; }
        public string Category { get; set; }
    }

    // ... (keep all your other existing methods like CreateEmojiButton, etc.)

}