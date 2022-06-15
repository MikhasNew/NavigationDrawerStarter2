using Android;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using EfcToXamarinAndroid.Core;
using Google.Android.Material.Badge;
using Google.Android.Material.Tabs;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using System.Collections.Generic;
using System.Linq;

namespace NavigationDrawerStarter
{
    public partial class MainActivity
    {
        class ViewPage2Fragment : AndroidX.Fragment.App.Fragment, ListView.IOnItemClickListener
        {
            public int Index { get; private set; }
            public List<DataItem> ListData { get;  set; }
            public DataAdapter DataAdapter { get; private set; } //tested
            private int[] ColorSum { get; set; } = new int[3] { 0x47, 0x07, 0x07 };
            private int[] ColorTransCount { get; set; } = new int[3] { 0x48, 0x59, 0x87 };
            public ViewPage2Fragment(int index, List<DataItem> listData)
            {
                Index = index;
                ListData = listData;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                #region 
                var ViewAD = LayoutInflater.Inflate(Resource.Layout.tab_layout, container, false);
                ListView listViewItems = ViewAD.FindViewById<ListView>(Resource.Id.dateslistView);

                listViewItems.OnItemClickListener = this;

                var listAdapter = new DataAdapter(this, ListData);
                DataAdapter = listAdapter; //test
                listViewItems.Adapter = listAdapter;

                listAdapter.OnDataSetChanged += (AndroidX.Fragment.App.Fragment context) => 
                {

                    TabLayout listView = (TabLayout)Activity.FindViewById(Resource.Id.tabLayout);
                    var tab = listView.GetTabAt(Index);

                  
                    int oldCount = 0;
                    int.TryParse(tab.Text.Split(":")[0], out oldCount);
                   
                    tab.SetText($"{ListData.Count}: Sum={ListData.Select(x => x.Sum).Sum()}");


                    //var orCreateBadge = tab?.OrCreateBadge;
                    //orCreateBadge.Number = 10;

                    //BadgeDrawable badgeDrawable = BadgeDrawable.Create(tab.View.Context);

                    var badge = tab.View.Tab.OrCreateBadge;
                    badge.BadgeGravity = BadgeDrawable.TopStart;
                    badge.Number = ListData.Count - oldCount;

                    //badgeDrawable.Number = ListData.Count - oldCount;
                    //var gravity = badgeDrawable.BadgeGravity; //8388661
                    //badgeDrawable.VerticalOffset = 50;
                    //badgeDrawable.SetVisible(true);
                    //BadgeUtils.AttachBadgeDrawable(badgeDrawable, tab.View);

                };

                TabLayout listView = (TabLayout)Activity.FindViewById(Resource.Id.tabLayout);
                listView.GetTabAt(Index).SetText($"{ListData.Count}: Sum={ListData.Select(x => x.Sum).Sum()}");
                return ViewAD;
                #endregion
            }

            public void OnItemClick(AdapterView parent, View view, int position, long id)
            {
                LayoutInflater inflater = Activity.LayoutInflater;
                var plot_layout = inflater.Inflate(Resource.Layout.plot_layout, null);
                var plotView = plot_layout.FindViewById<PlotView>(Resource.Id.plot_view);
                var totalSummText = plot_layout.FindViewById<TextView>(Resource.Id.TotalSummTextView);
                var totalTransText = plot_layout.FindViewById<TextView>(Resource.Id.TotalTransactionTextView);

                var selectedItem = ListData[position];
                var recurringDiscrCount = ListData.Where(x => x.Descripton == selectedItem.Descripton).Count();
                var recurringDiscrSumm = ListData.Where(x => x.Descripton == selectedItem.Descripton).Select(x => x.Sum).Sum();
                float shareOfTransactions = (float)recurringDiscrCount / ListData.Count * 100;
                float shareOfSumms = recurringDiscrSumm / ListData.Select(x => x.Sum).Sum() * 100;

                totalSummText.SetTextColor(Android.Graphics.Color.Rgb(ColorSum[0], ColorSum[1], ColorSum[2]));
                totalTransText.SetTextColor(Android.Graphics.Color.Rgb(ColorTransCount[0], ColorTransCount[1], ColorTransCount[2]));
                totalSummText.Text = $"Сумма транзакций - {recurringDiscrSumm} ({shareOfSumms.ToString()}%)";
                totalTransText.Text = $"Количество транзакций - {recurringDiscrCount} ({shareOfTransactions.ToString()}%)";

                plotView.Model = CreatePlotModel(selectedItem.Descripton, shareOfSumms, shareOfTransactions);

                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);


                builder.SetCancelable(false);
                builder.SetIcon(Resource.Drawable.abc_ic_go_search_api_material);
                builder.SetView(plot_layout);
                builder.SetNegativeButton("OK", (c, ev) =>
                {
                    builder.Dispose();
                });
                builder.Create();
                builder.Show();
            }

            private PlotModel CreatePlotModel(string diskr, float sum, float count)
            {


                var plotModel1 = new PlotModel();
                plotModel1.Title = $"Транзакции {diskr}";
                //plotModel1.Background = OxyColors.LightGray;

                plotModel1.DefaultColors = new List<OxyColor>{
                    OxyColors.Green,
                    OxyColor.FromRgb((byte)ColorSum[0], (byte)ColorSum[1], (byte)ColorSum[2]),
                    OxyColor.FromRgb((byte)ColorTransCount[0], (byte)ColorTransCount[1], (byte)ColorTransCount[2]),
                };

                var pieSeries1 = new PieSeries();
                //pieSeries1.StartAngle = 90;
                pieSeries1.FontSize = 0;
                //pieSeries1.FontWeight = FontWeights.Bold;
                pieSeries1.TextColor = OxyColors.White;

                var ollCircle = 100 - sum - count;
                if (ollCircle > 0)
                    pieSeries1.Slices.Add(new PieSlice("", ollCircle));
                pieSeries1.Slices.Add(new PieSlice("", sum));
                pieSeries1.Slices.Add(new PieSlice("", count));

                plotModel1.Series.Add(pieSeries1);

                //plotModel1.Annotations.Add(new LineAnnotation { Slope = 0.1, Intercept = 1, Text = "LineAnnotation", ToolTip = "This is a tool tip for the LineAnnotation" });
                //plotModel1.Annotations.Add(new RectangleAnnotation { MinimumX = 20, MaximumX = 70, MinimumY = 10, MaximumY = 40, TextRotation = 10, Text = "RectangleAnnotation", ToolTip = "This is a tooltip for the RectangleAnnotation", Fill = OxyColor.FromAColor(99, OxyColors.Blue), Stroke = OxyColors.Black, StrokeThickness = 2 });
                //plotModel1.Annotations.Add(new EllipseAnnotation { X = 20, Y = 60, Width = 20, Height = 15, Text = "EllipseAnnotation", ToolTip = "This is a tool tip for the EllipseAnnotation", TextRotation = 10, Fill = OxyColor.FromAColor(99, OxyColors.Green), Stroke = OxyColors.Black, StrokeThickness = 2 });
                //plotModel1.Annotations.Add(new PointAnnotation { X = 50, Y = 50, Text = "P1", ToolTip = "This is a tool tip for the PointAnnotation" });
                //plotModel1.Annotations.Add(new ArrowAnnotation { StartPoint = new DataPoint(8, 4), EndPoint = new DataPoint(0, 0), Color = OxyColors.Green, Text = "ArrowAnnotation", ToolTip = "This is a tool tip for the ArrowAnnotation" });
                //plotModel1.Annotations.Add(new TextAnnotation { TextPosition = new DataPoint(60, 60), Text = "TextAnnotation", ToolTip = "This is a tool tip for the TextAnnotation" });

                //plotModel1.Series.Add(pieSeries1);

                //var ta = new TextAnnotation();
                //ta.Text = "Label Text";
                //ta.TextColor = OxyColors.Black;
                //ta.Stroke = OxyColors.Transparent;
                //ta.StrokeThickness = 5;
                //ta.FontSize = 36;
                //ta.TextPosition = new DataPoint(50, 50);
                //plotModel1.Annotations.Add(ta);

                return plotModel1;


            }
        }

    }

}
