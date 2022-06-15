using Google.Android.Material.Badge;
using Google.Android.Material.Tabs;
using static Google.Android.Material.Tabs.TabLayoutMediator;

namespace NavigationDrawerStarter
{
    public partial class MainActivity
    {

        public class CustomStrategy : Java.Lang.Object, ITabConfigurationStrategy
        {
            public void OnConfigureTab(TabLayout.Tab p0, int p1)
            {
                //p0.SetText(MainActivity.fragmentTitles[p1]);
                p0.SetIcon(MainActivity.tabIcons[p1]);
                //p0.SetText("The number of transactions and their amount will be displayed here");
                
                // p0.SetTabLabelVisibility(TabLayout.TabLabelVisibilityUnlabeled);
                p0.SetText(p0.Text);
               

            }
        }

    }

}

