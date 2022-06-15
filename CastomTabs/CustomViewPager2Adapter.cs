using AndroidX.Lifecycle;
using AndroidX.ViewPager2.Adapter;
using EfcToXamarinAndroid.Core;
using Google.Android.Material.Badge;
using System.Collections.Generic;

namespace NavigationDrawerStarter
{
    public partial class MainActivity
    {
        public class CustomViewPager2Adapter : FragmentStateAdapter
        {
            private AndroidX.Fragment.App.FragmentManager _fragmentManager;
            public CustomViewPager2Adapter(AndroidX.Fragment.App.FragmentManager fragmentManager, Lifecycle lifecycle) : base(fragmentManager, lifecycle)
            {
                _fragmentManager = fragmentManager;
            }
            public override int ItemCount => 3;
            private AndroidX.Fragment.App.Fragment fragment = new AndroidX.Fragment.App.Fragment();
            public override AndroidX.Fragment.App.Fragment CreateFragment(int position)
            {
                switch (position)
                {
                    case 0:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.GetPayments(DatesRepositorio.DataItems));
                        break;
                    case 1:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.GetDeposits(DatesRepositorio.DataItems));
                        break;
                    case 2:
                        fragment = new ViewPage2Fragment(position, DatesRepositorio.GetCashs(DatesRepositorio.DataItems));
                        break;
                }
                return fragment;
            }
            public void AddNewItemToFragments()
            {
               
                if (_fragmentManager.Fragments.Count==0)
                    return;
                for (int i = 0; i < _fragmentManager.Fragments.Count; i++)
                {
                    var ft = (ViewPage2Fragment)_fragmentManager.Fragments[i];
                    List<DataItem> newItems = new List<DataItem>(); ;
                    switch (i)
                    {
                        case 0:
                            newItems = DatesRepositorio.GetPayments(DatesRepositorio.NewDataItems);
                            break;
                        case 1:
                            newItems = DatesRepositorio.GetDeposits(DatesRepositorio.NewDataItems);
                            break;
                        case 2:
                            newItems =  DatesRepositorio.GetCashs(DatesRepositorio.NewDataItems);
                            break;
                    }

                    ft.ListData.AddRange(newItems);
                   
                    ft.DataAdapter.NotifyDataSetChanged();
                }
              
            }
        }

    }

}

