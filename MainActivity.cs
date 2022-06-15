using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.OS;
using Android.Runtime;

using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Google.Android.Material.Tabs;
using NavigationDrawerStarter.Configs.ManagerCore;
using NavigationDrawerStarter.Fragments;
using NavigationDrawerStarter.Parsers;
using Xamarin.Essentials;

namespace NavigationDrawerStarter
{
    [Android.App.Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public partial class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        DrawerLayout drawer;
        RightMenu _RightMenu;

        private static int[] tabIcons;

        TabLayout tabLayout;
        ViewPager2 pager;
        CustomViewPager2Adapter adapter;

        private static List<BankConfiguration> smsFilters = new List<BankConfiguration>();


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            #region Stock
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            //drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            #endregion

            #region ConfigManager
            ConfigurationManager configManager = ConfigurationManager.ConfigManager;
            var configuration = configManager.JSONConfiguration;
            #endregion

            #region ShowDataFromDb
            await DatesRepositorio.SetDatasFromDB();
            #endregion

            #region Castom Tab
           
            tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout);
            tabLayout.InlineLabel = true;
            tabLayout.TabGravity = 0;
           
            pager = FindViewById<ViewPager2>(Resource.Id.pager);

            tabLayout.TabSelected += (object sender, TabLayout.TabSelectedEventArgs e) =>
            {
                var tab = e.Tab;
                var layout = tab.View;

                var layoutParams = layout.LayoutParameters;// as AndroidX.AppCompat.Widget.LinearLayoutCompat.LayoutParams;
               
                tab.SetTabLabelVisibility(TabLayout.TabLabelVisibilityLabeled);

                layoutParams.Width = LinearLayoutCompat.LayoutParams.WrapContent;
                
                layout.LayoutParameters = layoutParams;
            };
            tabLayout.TabUnselected += (object sender, TabLayout.TabUnselectedEventArgs e) =>
            {
                e.Tab.RemoveBadge();

                var tab = e.Tab;
                var layout = tab.View;
                tab.SetTabLabelVisibility(TabLayout.TabLabelVisibilityUnlabeled);
                // layoutParams.Width = LinearLayout.LayoutParams.WrapContent;
            };


            adapter = new CustomViewPager2Adapter(this.SupportFragmentManager, this.Lifecycle);
            tabIcons = new int[]{
            Resource.Mipmap.ic_cash50,
            Resource.Mipmap.ic_in_deposit50,
            Resource.Mipmap.ic_cash_out
            };
            pager.Adapter = adapter;

            new TabLayoutMediator(tabLayout, pager, new CustomStrategy()).Attach();
            adapter.NotifyDataSetChanged();
            //var fragment1 = (ViewPage2Fragment)SupportFragmentManager.FindFragmentById(0);
            //var kjh = fragment1.ListData;
            #endregion

            #region ReadSmS
            //smsFilters.AddRange(configuration.Banks); //This operation took 5420
            //List<Sms> lst = await GetAllSmsAsync(smsFilters);// This operation took 1356
            await ParseSmsToDbAsync(configuration.Banks);//This operation took 56

            #endregion
        }

        private async Task ParseSmsToDbAsync(List<BankConfiguration> bankConfigurations)
        {
            SmsReader smsReader = new SmsReader(this);
            List<Sms> smsList = await smsReader.GetAllSmsAsync(bankConfigurations);
            
            Parser parserBelarusbank = new Parser(smsList, bankConfigurations);//This operation took 3558
            var data = parserBelarusbank.GetData();
            if (data != null)
            {
                await DatesRepositorio.AddDatas(data);//This operation took 10825
                adapter.AddNewItemToFragments();
            }
            //adapter.NotifyDataSetChanged();
        }

        async Task<FileResult> PickAndShow(PickOptions options)
        {
            #region ConfigManager
            ConfigurationManager configManager = ConfigurationManager.ConfigManager;
            var configuration = configManager.JSONConfiguration;
            #endregion
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    //Text = $"File Name: {result.FileName}";
                    //if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    //    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    var stream = await result.OpenReadAsync();
                    //    Image = ImageSource.FromStream(() => stream);
                    //}
                    Parser parserBelarusbank = new Parser(result.FullPath, configuration.Banks);//This operation took 3558

                    var data = await parserBelarusbank.GetDataFromPdf();
                    await DatesRepositorio.AddDatas(data);//This operation took 10825
                    adapter.AddNewItemToFragments();

                    //adapter.NotifyDataSetChanged();

                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        #region Stock
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            #region FilterBtnClic
            if (id == Resource.Id.action_openRight)
            {
                if (_RightMenu == null)
                {
                    _RightMenu = new RightMenu();

                    var filterFragmentTransaction = SupportFragmentManager.BeginTransaction();
                    filterFragmentTransaction.Add(Resource.Id.MenuFragmentFrame, _RightMenu, "MENU");
                    filterFragmentTransaction.Commit();
                    _RightMenu.FiltredList = DatesRepositorio.DataItems.Select(x=>x.Descripton).ToList<string>();
                    drawer.OpenDrawer(GravityCompat.End);

                    _RightMenu.SetFilters += (object sender, EventArgs e) =>
                    {
                        var filter = ((RightMenu)sender).FilredResultList;
                        drawer.CloseDrawer(GravityCompat.End);
                        //drawer.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
                    };
                    return true;
                }
                else
                {
                    drawer.OpenDrawer(GravityCompat.End);
                }
            }
            #endregion

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_import)
            {
                var options = new PickOptions
                {
                    PickerTitle = "@string/select_pdf_report"
                    //FileTypes = customFileType,
                };

                PickAndShow(options);
            }
            //else if (id == Resource.Id.nav_gallery)
            //{
            //    var menuTransaction = SupportFragmentManager.BeginTransaction();
            //    menuTransaction.Replace(Resource.Id.fragment_container, new Fragment1(), "Fragment1");
            //    menuTransaction.Commit();
            //}
            //else if (id == Resource.Id.nav_slideshow)
            //{
            //    var menuTransaction = SupportFragmentManager.BeginTransaction();
            //    menuTransaction.Replace(Resource.Id.fragment_container, new Fragment2(), "Fragment2");
            //    menuTransaction.Commit();
            //}
            //else if (id == Resource.Id.nav_manage)
            //{
            //    var welcomeTransaction = SupportFragmentManager.BeginTransaction();
            //    welcomeTransaction.Replace(Resource.Id.fragment_container, new Welcome(), "Welcome");
            //    welcomeTransaction.Commit();
            //}
            //else if (id == Resource.Id.nav_share)
            //{
            //    var welcomeTransaction = SupportFragmentManager.BeginTransaction();
            //    welcomeTransaction.Replace(Resource.Id.fragment_container, new Welcome(), "Welcome");
            //    welcomeTransaction.Commit();
            //}
            //else if (id == Resource.Id.nav_send)
            //{
            //    var welcomeTransaction = SupportFragmentManager.BeginTransaction();
            //    welcomeTransaction.Replace(Resource.Id.fragment_container, new Welcome(), "Welcome");
            //    welcomeTransaction.Commit();
            //}

            //DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        #endregion

        ///////////////////////
        ///tested
        ///
       
    }
}

