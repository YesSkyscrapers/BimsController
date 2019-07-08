using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Defines
{
    public static class Errors
    {
        public static Error FailedLoadAppSettings = new Error(0, "Не удалось загрузить настройки приложения");
        public static Error FailedSaveAppSettings = new Error(1, "Не удалось сохранить настройки приложения");
    }
}
