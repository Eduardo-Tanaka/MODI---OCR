using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OcrMODI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // método para redimensionar e alterar o dpi da imagem para 300
        private Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(300, 300);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void btnOcr_Click(object sender, EventArgs e)
        {
            try
            {
                // Abre um OpenFileDialog para o usuário escolher uma imagem em formato .tif
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "tif|*.tif";
                openFileDialog.Title = "Selecione uma imagem no formato .tif";

                // Mostra o Dialog
                // Se o usuário clicar em OK, abre a imagem e exibe no pictureBox
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Image image = Image.FromFile(openFileDialog.FileName);
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // diminui ou aumenta a imagem para caber no espaço do pictureBox

                    int activePage;
                    int pages;

                    pages = image.GetFrameCount(FrameDimension.Page); // pega a quantidade de páginas da imagem

                    btnOcr.Enabled = false; // desabilita o botão

                    // itera por todas as páginas
                    for (int index = 0; index < pages; index++)
                    {
                        activePage = index + 1; // página atual da imagem

                        image.SelectActiveFrame(FrameDimension.Page, index); // seleciona a página atual

                        // exibe a imagem no picture box
                        pictureBox.Image = image;
                        pictureBox.Refresh();

                        string fileName = Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName) + "_Page_" + activePage.ToString() + ".tif";

                        // Redimensiona a imagem para um tamanho padrão(2400px de largura e 3500px de altura) e para 300dpi
                        Bitmap newBitmap = new Bitmap(ResizeImage(image, 2480, 3500));
                        newBitmap.SetResolution(300, 300);
                        newBitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Tiff); // salva a imagem

                        // cria o documento do imaging
                        MODI.Document md = new MODI.Document();
                        try
                        {
                            md.Create(fileName);
                            // adiciona o método que verifica o progresso do OCR
                            md.OnOCRProgress += new MODI._IDocumentEvents_OnOCRProgressEventHandler(this.ShowProgress);
                            // vira e alinha o texto
                            md.Images[0].OCR(MODI.MiLANGUAGES.miLANG_PORTUGUESE, true, true);
                            MODI.Image img = (MODI.Image)md.Images[0];
                            MODI.Layout layout = img.Layout;
                            
                            // coloca o texto do OCR na variável abaixo
                            string textoOcr = layout.Text;

                            // Grava o resultado do OCR em um arquivo txt
                            using (StreamWriter writer = new StreamWriter(Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName) + ".txt", true))
                            {
                                writer.WriteLine("Página: " + activePage);
                                writer.WriteLine(textoOcr);
                            }

                            // divide o texto em um array para poder iterar por cada linha
                            string[] array = Regex.Split(textoOcr, "\r\n");
                            foreach (var linha in array)
                            {
                                // caso quiser realiza a lógica linha por linha
                            }

                            // exibe o texto do OCR no textBox do lado da imagem
                            textBox1.Text = textoOcr;
                            textBox1.Refresh();
                        }
                        catch (Exception ex)
                        {
                            // Caso ocorra um erro mostra a mensagem
                            // Se tentar realizar o OCR 2 vezes seguidas na mesma imagem irá ocorrer um erro, use imagens diferentes em cada vez para não ocorrer o erro.
                            // Há um bug no MODI e ele não libera a imagem imediatamente ao fechar, então se você tentar passar o OCR na mesma imagem seguidamente irá dar o erro.
                            MessageBox.Show(ex.Message);
                        }

                        // libera os recursos da imagem
                        newBitmap.Dispose();
                        // fecha o documento
                        md.Close();
                        md = null;
                   
                        // roda o garbage colector para liberar memória
                        GC.Collect();
                        GC.WaitForFullGCComplete();
                    }

                    //pictureBox.Image = null;
                    //pictureBox.Refresh();

                    image.Dispose();
                    openFileDialog.Dispose();

                    btnOcr.Enabled = true; // habilita o botão
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnOcr.Enabled = true;
            }
        }

        // método para verificar o progresso do OCR
        public void ShowProgress(int progress, ref bool cancel)
        {
            lblProgOcr.Text = "OCR:  => " + progress.ToString() + "% processado.";
        }
    }
}
