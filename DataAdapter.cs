using Android;
using Android.Views;
using Android.Widget;
using EfcToXamarinAndroid.Core;
using System.Collections.Generic;

namespace NavigationDrawerStarter
{
    public class DataAdapter : BaseAdapter<DataItem>
    {
        private readonly AndroidX.Fragment.App.Fragment context;
        private readonly List<DataItem> dataItems;

        public delegate void DataAdapterHandler(AndroidX.Fragment.App.Fragment context);
        public event DataAdapterHandler? OnDataSetChanged;

        public DataAdapter(AndroidX.Fragment.App.Fragment context, List<DataItem> dataItems)
        {
            this.context = context;
            this.dataItems = dataItems;
        }

        public override DataItem this[int position]
        {
            get
            {
                return dataItems[position];
            }
        }

        public override int Count
        {
            get
            {
                return dataItems.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return dataItems[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? context.LayoutInflater.Inflate(Resource.Layout.list_item, parent, false);// inflate the xml for each item

            var txtTitulo = view.FindViewById<TextView>(Resource.Id.tituloTextView);
            var txtDiretor = view.FindViewById<TextView>(Resource.Id.diretorTextView);
            var txtLancamento = view.FindViewById<TextView>(Resource.Id.dataLancamentoTextView);

            txtTitulo.Text = dataItems[position].Sum.ToString();
            txtDiretor.Text = dataItems[position].Descripton;
            txtLancamento.Text = dataItems[position].Date.ToShortDateString();

            return view;
        }

        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
            OnDataSetChanged?.Invoke(context);
        }


    }
}