using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = true;
            BtnProcessar.IsEnabled = false;
            _cts = new CancellationTokenSource();
            var inicio = DateTime.Now;
            var contas = r_Repositorio.GetContaClientes();
            PgsProgresso.Maximum = contas.Count();
            

            LimparView();

            var progresso = new Progress<string>(str => PgsProgresso.Value++);
            try
            {
                var resultado = await ConsolidarContas(contas, progresso, _cts.Token);

                var fim = DateTime.Now;
                AtualizarView(resultado, fim - inicio);
            }
            catch (Exception)
            {
                TxtTempo.Text = "Operação foi cancelada pelo usuário";
            }
            
            BtnProcessar.IsEnabled = true;
            BtnCancelar.IsEnabled = false;

            await Task.Factory.StartNew(() => { }).ContinueWith(t1 => { });

        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            _cts.Cancel();
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
            PgsProgresso.Value = 0;
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> cliente, IProgress<string> reportadorDeProgresso, CancellationToken ct)
        {
            var tasks = cliente.Select(conta =>            
                Task.Factory.StartNew(() => {
                    ct.ThrowIfCancellationRequested();

                    var resultadoConsolidacao = r_Servico.ConsolidarMovimentacao(conta);
                    reportadorDeProgresso.Report(resultadoConsolidacao);

                    ct.ThrowIfCancellationRequested();

                    return resultadoConsolidacao;
                }, ct)
            );

            return await Task.WhenAll(tasks);
        }

        private void AtualizarView(IEnumerable<string> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
