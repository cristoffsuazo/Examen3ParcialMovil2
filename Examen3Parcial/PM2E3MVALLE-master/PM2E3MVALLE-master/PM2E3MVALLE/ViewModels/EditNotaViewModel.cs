using Firebase.Database;
using Firebase.Storage;
using Firebase.Database.Query;
using PM2E3MVALLE.Models;
using System.Windows.Input;

namespace PM2E3MVALLE.ViewModels
{
    public class EditNotaViewModel : BindableObject
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://examenluisc-default-rtdb.firebaseio.com/");
        private Nota _nota;

        public Nota Nota
        {
            get => _nota;
            set
            {
                _nota = value;
                OnPropertyChanged();
            }
        }

        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand SeleccionarCommand { get; set; }
        private string UrlFoto { get; set; }
        private string UrlAudio { get; set; }

        public EditNotaViewModel()
        {
            GuardarCommand = new Command(async () => await OnGuardarCommand());
            CancelarCommand = new Command(async () => await OnCancelarCommand());
            SeleccionarCommand = new Command(async () => await OnSeleccionarCommand());
        }

        public async Task LoadNotaAsync(string name)
        {
            // Cargar la nota desde Firebase
            var notas = await _client.Child("Notas").OnceAsync<Nota>();
            var nota = notas.FirstOrDefault(p => p.Object.Descripcion == name);
            if (nota != null)
            {
                Nota = nota.Object;
            }
        }

        private async Task OnGuardarCommand()
        {
            try
            {
                if (Nota != null)
                {

                    var notas = await _client.Child("Notas").OnceAsync<Nota>();

                    // Verifica si la nota existe en la base de datos
                    var notaFirebase = notas
                        .FirstOrDefault(p => p.Object.Descripcion?.Trim().Equals(Nota.Descripcion?.Trim(), StringComparison.OrdinalIgnoreCase) == true);


//                    var notas = await _client.Child("Notas").OnceAsync<Nota>();
//                    var notaFirebase = notas.FirstOrDefault(p => p.Object.Descripcion == Nota.Descripcion);
                 
                    if (notaFirebase != null)
                    {
                        await _client.Child("Notas").Child(notaFirebase.Key).PutAsync(Nota);

                        // Enviar mensaje de notificación
                        MessagingCenter.Send(this, "NotaActualizada");

                        await Shell.Current.GoToAsync(".."); // Navegar hacia atrás
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Nota no encontrada en la base de datos", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "La nota es nula", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }


        private async Task OnCancelarCommand()
        {
            await Shell.Current.GoToAsync(".."); // Navegar hacia atrás
        }

        private async Task OnSeleccionarCommand()
        {
            try
            {

                var foto = await MediaPicker.PickPhotoAsync();
                if (foto != null)
                {
                    var stream = await foto.OpenReadAsync();
                    UrlFoto = await new FirebaseStorage("examenluisc.appspot.com")
                                    .Child("Photos")
                                    .Child(DateTime.Now.ToString("ddMMyyhhmmss") + foto.FileName)
                                    .PutAsync(stream);

                    // Actualiza la nota la URL de la foto
                    Nota.Foto = UrlFoto;

                    OnPropertyChanged(nameof(Nota)); // Notificar que nota ha sido cambiada
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error al seleccionar la foto: {ex.Message}", "OK");
            }
        }

    }
}