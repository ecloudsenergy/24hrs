using sample2.models;
using sample2.remote;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using System.Windows.Threading;
using sample2.helpers;
using System.Threading;

namespace sample2
{
    /// <summary>
    /// Interaction logic for _15_Cash_Acceptor_Log.xaml
    /// </summary>
    public partial class _15_Cash_Acceptor_Log : Page
    {

        Page previous_page;

        LogModel user_details = SqliteDataAccess.getLastLogEvent();
        DenominationRecords records = new DenominationRecords();
        List<int> denominations = new List<int>();
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        static Notifier noti = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomCenter,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(2),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        public _15_Cash_Acceptor_Log(Page previous_page)
        {
            InitializeComponent();
            this.previous_page = previous_page;
            this.username.Text = user_details.LT_username;
            routines.Denominations(denominations);
            records = SqliteDataAccess.getAllDenominationDetails(denominations);


            //   this.Text = "" + balance_amount;
            startClock();


        }


        private void startClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += tick_event;
            timer.Start();
        }

        private void tick_event(object sender, EventArgs e)
        {
            this.Time_Now.Text = indianTime.ToShortDateString() + " " + indianTime.ToShortTimeString() + " hrs";
        }




        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Back_Button(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(previous_page);
        }



        /*   private void input_TextChanged(object sender, TextChangedEventArgs e)
           {
               int removed_nos = 0;
               TextBox input = sender as TextBox;
               string[] names = input.Name.Split('_');
               if (input.Text == "") removed_nos = 0;
               else removed_nos = Int32.Parse(input.Text);
               if (balance_nos_all.Count > 0)
                   calculate(names[2], removed_nos, input);
               else
               {
                   noti.ShowError("No Balance Notes Available!");

                   MessageBox.Show("No Balance Notes Available!", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                   input.Text = "";
               }
           }*/

        /*  private void calculate(string code, int removed_nos, TextBox input)
          {
              TextBox Removed_Amount = (TextBox)this.FindName("removed_amount_" + code);
              int denomination = Int32.Parse(code);


                  int balance_nos = balance_nos_all[denominations.IndexOf(denomination)] - removed_nos;
                  if (balance_nos >= 0)
                  {
                      int removed_amount = removed_nos * denomination;
                      Removed_Amount.Text = "" + removed_amount;
                      int total_removed_amount = 0;
                      for (int i = 0; i < denominations.Count; i++)
                      {
                          TextBox Removed_Amount_Individual = (TextBox)this.FindName("removed_amount_" + denominations[i]);

                          total_removed_amount += Int32.Parse(Removed_Amount_Individual.Text);
                      }


                   //  this.total_removed_amount.Text = "" + total_removed_amount;
                  //    int final_balance = this.balance_amount - total_removed_amount;
                  //   this.Final_Balance.Text = "" + final_balance;
                  input.Background = new SolidColorBrush(Colors.Transparent);
              }

              else
                      input.Background = new SolidColorBrush(Colors.LightSalmon);

          }*/

        /* private void Save_Button(object sender, RoutedEventArgs e)
           {
               bool error = false;
              if (this.Status_Txt.Text == "") noti.ShowError("Please enter the request number.");
               else
               {
                   for (int i = 0; i < denominations.Count; i++)
                   {
                       TextBox Removed_Nos_Individual = (TextBox)this.FindName("removed_nos_" + denominations[i]);
                       SolidColorBrush background = Removed_Nos_Individual.Background as SolidColorBrush;

                       if (Colors.LightSalmon.Equals(background.Color))
                       {
                           error = true;
                           break;
                       }
                   }

                   if (error || balance_nos_all.Count == 0)
                   {
                       noti.ShowError("Fill Correct Data! Change Highlighted TextBox!");
                       MessageBox.Show("Fill Correct Data! Change Highlighted TextBox!", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                   }
                   else
                   {
                       int saved_successfully = 0;
                       int attempted = 0;
                       for (int i = 0; i < denominations.Count; i++)
                       {
                           TextBox Removed_Amount_Individual = (TextBox)this.FindName("removed_amount_" + denominations[i]);
                           TextBox Removed_Nos_Individual = (TextBox)this.FindName("removed_nos_" + denominations[i]);
                           NoteAcceptorModel noteLog = new NoteAcceptorModel();
                           if (Removed_Amount_Individual.Text == "") noteLog.NA_Amount = 0;
                           else
                               noteLog.NA_Amount = Int32.Parse(Removed_Amount_Individual.Text);

                           if (noteLog.NA_Amount > 0)
                           {
                               String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss");//imthi 20-01-2021
                               noteLog.NA_Balance = (balance_nos_all[denominations.IndexOf(denominations[i])] * denominations[i]) - noteLog.NA_Amount;
                               noteLog.NA_Denomination = denominations[i];
                               noteLog.NA_Quantity = noteLog.NA_Balance / denominations[i];
                               noteLog.NA_TransactionID = user_details.LT_username;
                               saved_successfully += SqliteChange.InsertIntoCurrencyTransactionTable(noteLog.NA_Denomination, this.Status_Txt.Text, Int32.Parse(Removed_Nos_Individual.Text),
                                   formatdate, SqliteDataAccess.getLastLogEvent().LT_username, "Debit", balance_nos_all[denominations.IndexOf(denominations[i])], //imthi 20-01-2021
                                  noteLog.NA_Quantity, "BA","Completed");
                              attempted++;
                           }

                       }
                       if (saved_successfully != attempted && attempted != 0)
                       {
                           noti.ShowError("Save Failed.");
                           //MessageBox.Show("Save Failed.");
                       }

                       else if (attempted == 0)
                       {
                           noti.ShowError("Please enter atleast one field.");
                           //MessageBox.Show("Please enter atleast one field.");
                       }
                       else
                       {
                           noti.ShowSuccess("Saved Successfully.");
                           //MessageBox.Show("Saved Successfully.");
                           this.NavigationService.Navigate(new _15_Cash_Acceptor_Log(previous_page));

                       }
                   }
               }
           } */

        private void Reset_Button(object sender, RoutedEventArgs e)
        {

            Credit_5.Text = ""; Credit_10.Text = ""; Credit_20.Text = ""; Credit_50.Text = ""; Credit_100.Text = ""; Credit_200.Text = ""; Credit_500.Text = ""; Credit_2000.Text = "";
            Debit_5.Text = ""; Debit_10.Text = ""; Debit_20.Text = ""; Debit_50.Text = ""; Debit_100.Text = ""; Debit_200.Text = ""; Debit_500.Text = ""; Debit_2000.Text = "";
            Request_type.Text = "Select Option";
            Remark.Text = "";

        }


        private void Total_Credited_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Total_Debited_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void REMARK_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        int finalcredti_amount = 0;
        public void Credit_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<int> denominations = new List<int>();
            routines.Denominations(denominations);
            finalcredti_amount = 0;
            for (int i = 0; i < denominations.Count; i++)
            {
                TextBox creditqty = (TextBox)this.FindName("Credit_" + denominations[i]);
                int Qty = 0;
                if (creditqty.Text != "")
                    Qty = int.Parse(creditqty.Text);
                int total = Qty * denominations[i];
                finalcredti_amount += total;
            }
            this.Total_Credited.Text = finalcredti_amount.ToString();
        }
        int finaldebit_amount = 0;


        private void Debit_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<int> denominations = new List<int>();
            routines.Denominations(denominations);
            finaldebit_amount = 0;
            for (int i = 0; i < denominations.Count; i++)
            {
                TextBox debitqty = (TextBox)this.FindName("Debit_" + denominations[i]);
                int dQty = 0;
                if (debitqty.Text != "")
                    dQty = int.Parse(debitqty.Text);
                int total = dQty * denominations[i];
                finaldebit_amount += total;

            }
            this.Total_Debited.Text = finaldebit_amount.ToString();
        }
        private void number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void Status_Txt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Save_Button(object sender, RoutedEventArgs e)
        {
            if (Request_type.Text != "Select Option" )
            {
                Verify_popup.IsOpen = true;
                this.IsEnabled = false;



                List<int> denominations = new List<int>();
                routines.Denominations(denominations);
                string DBqty = String.Empty;
                string CDqty = String.Empty;
                List<Verified_deno_nos> Vrfy_DENO = new List<Verified_deno_nos>();
                for (int i = 0; i < denominations.Count; i++)
                {
                    Verified_deno_nos Verified = new Verified_deno_nos();
                    TextBox Debitqty = (TextBox)this.FindName("Debit_" + denominations[i]);
                    TextBox Creditqty = (TextBox)this.FindName("Credit_" + denominations[i]);
                    DBqty = Debitqty.Text;
                    CDqty = Creditqty.Text;
                    if (DBqty != "" || CDqty != "")
                    {

                        Verified.Debitqty1 = DBqty;
                        Verified.Creditqty1 = CDqty;
                        Verified.Denomination1 = denominations[i];
                        Vrfy_DENO.Add(Verified);
                    }
                }

                Verify_dynamic head = new Verify_dynamic();
                head.Deno.Text = "";
                head.Dbdqty.Text = "";
                head.Crdqty.Text = "PLEASE  CONFIRM";
                this.Confirm_list.Children.Add(head);

                Verify_dynamic header = new Verify_dynamic();
                header.Deno.Text = "Denomination";
                header.Dbdqty.Text = "Debit";
                header.Crdqty.Text = "Credit";
                this.Confirm_list.Children.Add(header);


                for (int i = 0; i < Vrfy_DENO.Count; i++)
                {
                    Verify_dynamic verfy_deno_nos_text = new Verify_dynamic();


                    if (verfy_deno_nos_text.Dbdqty.Text != "" && verfy_deno_nos_text.Deno.Text != "")
                    {
                        verfy_deno_nos_text.Deno.Text = Vrfy_DENO[i].Denomination1.ToString(); ;
                        verfy_deno_nos_text.Dbdqty.Text = Vrfy_DENO[i].Debitqty1;
                        verfy_deno_nos_text.Crdqty.Text = Vrfy_DENO[i].Creditqty1;
                        this.Confirm_list.Children.Add(verfy_deno_nos_text);
                    }





                }
            }


            else
            {
                noti.ShowError("Please select transaction type");
            }






        }
        /*    private void VERIFY(List<Verified_deno_nos> Check_DENO)

            {
                for(int i = 0; i < Check_DENO.Count; i++)
                {
                    Verify_dynamic verfy_deno_nos_text = new Verify_dynamic();
                    verfy_deno_nos_text.Deno.Text = Check_DENO[i].Denomination1.ToString();
                    verfy_deno_nos_text.Dbdqty.Text = Check_DENO[i].Debitqty1;
                    verfy_deno_nos_text.Crdqty.Text = Check_DENO[i].Creditqty1;
                    this.Confirm_list.Children.Add(verfy_deno_nos_text);


                }
            }



           /// <summary>
           /// 
           /// </summary>
           /// <returns></returns>





        */







        private void Hide_Click(object sender, RoutedEventArgs e)
        {

            Verify_popup.IsOpen = false;
            this.IsEnabled = true;
            List<Verified_deno_nos> Vrfy_DENO = new List<Verified_deno_nos>();



            Verify_dynamic verfy_deno_nos_text = new Verify_dynamic();
            while (Confirm_list.Children.Count > 0)
            {

                this.Confirm_list.Children.RemoveAt(Confirm_list.Children.Count - 1);
            }



        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            int Saving1 = 0, Saving2 = 0;


            int openingbal;
            int closingbal;


            if (Request_type.Text != "Select Option" && this.Total_Debited.Text != "" | this.Total_Credited.Text != "")
            {


                {
                    List<int> denominations = new List<int>();
                    routines.Denominations(denominations);
                    for (int i = 0; i < denominations.Count; i++)
                    {
                        TextBox creditqty = (TextBox)this.FindName("Credit_" + denominations[i]);
                        TextBox Debitqty = (TextBox)this.FindName("Debit_" + denominations[i]);


                        if (creditqty.Text != "" & Debitqty.Text != "" )
                        {
                            openingbal = SqliteChange.getBalance("BA", denominations[i]);

                            closingbal = openingbal + int.Parse(creditqty.Text);

                            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss");
                            Saving1 = SqliteChange.InsertIntoCurrencyTransactionTable(denominations[i], Request_type.Text, int.Parse(creditqty.Text),
                                formatdate, this.username.Text, "Credit", openingbal, closingbal, "BA", "Completed", this.Remark.Text);

                            closingbal = openingbal - int.Parse(Debitqty.Text);

                            Thread.Sleep(1000);
                            indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                            string formatdate2 = indianTime.ToString("yyyy-MM-dd HH:mm:ss");
                            Saving2 = SqliteChange.InsertIntoCurrencyTransactionTable(denominations[i], Request_type.Text, int.Parse(Debitqty.Text),
                                formatdate2, this.username.Text, "Debit", openingbal, closingbal, "BA", "Completed", this.Remark.Text);
                        }


                        else if (Debitqty.Text != "")
                        {

                            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss");
                            openingbal = SqliteChange.getBalance("BA", denominations[i]);
                            closingbal = openingbal - int.Parse(Debitqty.Text);
                            Saving2 = SqliteChange.InsertIntoCurrencyTransactionTable(denominations[i], Request_type.Text, int.Parse(Debitqty.Text),
                               formatdate, this.username.Text, "Debit", openingbal, closingbal, "BA", "Completed", this.Remark.Text);
                        }

                        else if (creditqty.Text != "")
                        {
                            openingbal = SqliteChange.getBalance("BA", denominations[i]);
                            closingbal = openingbal + int.Parse(creditqty.Text);
                            String formatdate = indianTime.ToString("yyyy-MM-dd HH:mm:ss");
                            Saving1 = SqliteChange.InsertIntoCurrencyTransactionTable(denominations[i], Request_type.Text, int.Parse(creditqty.Text),
                                formatdate, this.username.Text, "Credit", openingbal, closingbal, "BA", "Completed", this.Remark.Text);
                        }
                    }
                    noti.ShowSuccess("SAVED SUCESSFULLY");

                }
            }
            else
            {
                noti.ShowError("Please Enter value in Credit or Debit");
            }
            this.NavigationService.Navigate(previous_page);
        }
    }
}
