using System.Collections.Generic;

namespace SPTLeaderboard.Data;

public class LocalizationData
{
    public static Dictionary<string, string> Error_Banned = new Dictionary<string, string> // TODO: Get error code for this
    {
        { "ch", "您因违反排行榜规则而被封禁" },
        { "cz", "Byli jste zabanováni za porušení pravidel žebříčku" },
        { "en", "You have been banned for violating the Leaderboard rules" },
        { "fr", "Vous avez été banni pour avoir enfreint les règles du classement" },
        { "ge", "Sie wurden wegen Verstoßes gegen die Bestenlistenregeln gesperrt" },
        { "hu", "Ki lettél tiltva a ranglista szabályainak megsértése miatt" },
        { "it", "Sei stato bannato per aver violato le regole della classifica" },
        { "jp", "ランキングのルール違反によりBANされました" },
        { "kr", "리더보드 규칙 위반으로 인해 이용이 제한되었습니다" },
        { "pl", "Zostałeś zbanowany za naruszenie zasad rankingu" },
        { "po", "Você foi banido por violar as regras da tabela de classificação" },
        { "sk", "Boli ste zabanovaní za porušenie pravidiel rebríčka" },
        { "es", "Has sido baneado por infringir las reglas del leaderboard" },
        { "es-mx", "Has sido baneado por violar las reglas del leaderboard" },
        { "tu", "Liderlik tablosu kurallarını ihlal ettiğiniz için yasaklandınız" },
        { "ru", "Вы забанены за нарушение правил Leaderboard" },
        { "ro", "Ai fost interzis pentru încălcarea regulilor clasamentului" }
    };
        
    public static Dictionary<string, string> Error_LA_TOS = new Dictionary<string, string> // 699
    {
        { "ch", "排行榜条目被拒。违反许可协议/服务条款 1.2" },
        { "cz", "Záznam do žebříčku byl zamítnut. Porušení LA/TOS 1.2" },
        { "en", "Leaderboard entry denied. Violation of LA/TOS 1.2" },
        { "fr", "Entrée au classement refusée. Violation du CLUF/CGU 1.2" },
        { "ge", "Eintrag in die Bestenliste abgelehnt. Verstoß gegen LA/TOS 1.2" },
        { "hu", "Ranglista-bejegyzés elutasítva. LA/TOS 1.2 megsértése" },
        { "it", "Inserimento in classifica negato. Violazione di LA/TOS 1.2" },
        { "jp", "リーダーボードへの登録が拒否されました。LA/TOS 1.2の違反" },
        { "kr", "리더보드 등록이 거부되었습니다. LA/TOS 1.2 위반" },
        { "pl", "Wpis do rankingu odrzucony. Naruszenie LA/TOS 1.2" },
        { "po", "Entrada na tabela negada. Violação do LA/TOS 1.2" },
        { "sk", "Záznam do rebríčka zamietnutý. Porušenie LA/TOS 1.2" },
        { "es", "Entrada al ranking denegada. Violación de LA/TOS 1.2" },
        { "es-mx", "Entrada al leaderboard denegada. Violación de LA/TOS 1.2" },
        { "tu", "Liderlik tablosuna giriş reddedildi. LA/TOS 1.2 ihlali" },
        { "ru", "Доступ к таблице лидеров отклонён. Нарушение LA/TOS 1.2" },
        { "ro", "Intrarea în clasament a fost refuzată. Încălcarea LA/TOS 1.2" }
    };
    
    public static Dictionary<string, string> Error_TokenMismatch = new Dictionary<string, string> // 700
    {
        { "ch", "令牌不适用于当前资料。它可能正被其他玩家使用。" },
        { "cz", "Token neodpovídá aktuálnímu profilu. Možná jej používá jiný hráč." },
        { "en", "The token does not match the current profile. It may be used by another player." },
        { "fr", "Le jeton ne correspond pas au profil actuel. Il est peut-être utilisé par un autre joueur." },
        { "ge", "Das Token passt nicht zum aktuellen Profil. Es könnte von einem anderen Spieler verwendet werden." },
        { "hu", "A token nem illik a jelenlegi profilhoz. Lehet, hogy egy másik játékos használja." },
        { "it", "Il token non è valido per il profilo attuale. Potrebbe essere utilizzato da un altro giocatore." },
        { "jp", "トークンは現在のプロファイルに適していません。他のプレイヤーが使用している可能性があります。" },
        { "kr", "토큰이 현재 프로필에 적합하지 않습니다. 다른 플레이어가 사용 중일 수 있습니다." },
        { "pl", "Token nie pasuje do bieżącego profilu. Może być używany przez innego gracza." },
        { "po", "O token não corresponde ao perfil atual. Pode estar a ser utilizado por outro jogador." },
        { "sk", "Token nie je vhodný pre aktuálny profil. Možno ho používa iný hráč." },
        { "es", "El token no corresponde al perfil actual. Puede que lo esté usando otro jugador." },
        { "es-mx", "El token no es válido para el perfil actual. Es posible que otro jugador lo esté utilizando." },
        { "tu", "Belirteç mevcut profil için geçerli değil. Başka bir oyuncu tarafından kullanılıyor olabilir." },
        { "ru", "Токен не подходит для текущего профиля. Возможно, он используется другим игроком." },
        { "ro", "Tokenul nu este potrivit pentru profilul curent. Este posibil să fie folosit de un alt jucător." }
    };
    
    public static Dictionary<string, string> Error_TokenNotSafe = new Dictionary<string, string> // 702
    {
        { "ch", "您的令牌被拒绝为不安全。请尝试另一个或重新生成。" },
        { "cz", "Váš token byl zamítnut jako nebezpečný. Zkuste jiný nebo jej znovu vygenerujte." },
        { "en", "Your token was rejected as unsafe. Try a different one or generate a new one." },
        { "fr", "Votre jeton a été rejeté comme non sécurisé. Essayez-en un autre ou générez-en un nouveau." },
        { "ge", "Ihr Token wurde als unsicher abgelehnt. Versuchen Sie einen anderen oder generieren Sie einen neuen." },
        { "hu", "A token elutasításra került, mert nem biztonságos. Próbáljon meg egy másikat, vagy generáljon egy újat." },
        { "it", "Il tuo token è stato rifiutato perché non sicuro. Prova un altro o generane uno nuovo." },
        { "jp", "トークンは安全でないため拒否されました。別のものを試すか、新しく生成してください。" },
        { "kr", "토큰이 안전하지 않아 거부되었습니다. 다른 토큰을 시도하거나 새로 생성하세요." },
        { "pl", "Twój token został odrzucony jako niebezpieczny. Spróbuj innego lub wygeneruj nowy." },
        { "po", "O seu token foi rejeitado por ser inseguro. Tente outro ou gere um novo." },
        { "sk", "Váš token bol odmietnutý ako nebezpečný. Skúste iný alebo ho vygenerujte znova." },
        { "es", "Tu token fue rechazado por ser inseguro. Prueba con otro o genera uno nuevo." },
        { "es-mx", "Tu token fue rechazado por motivos de seguridad. Intenta con otro o genera uno nuevo." },
        { "tu", "Belirteciniz güvensiz olarak reddedildi. Başka birini deneyin veya yeniden oluşturun." },
        { "ru", "Ваш токен отклонён как небезопасный. Попробуйте другой или сгенерируйте заново." },
        { "ro", "Tokenul dvs. a fost respins ca nesigur. Încercați altul sau generați unul nou." }
    };
    
    public static Dictionary<string, string> Error_UpdateMod = new Dictionary<string, string> // 705
    {
        { "ch", "请更新模组到最新版本以提交结果。" },
        { "cz", "Aktualizujte prosím mod na nejnovější verzi pro odeslání výsledků." },
        { "en", "Please update the mod to the latest version to submit results." },
        { "fr", "Veuillez mettre à jour le mod vers la dernière version pour envoyer les résultats." },
        { "ge", "Bitte aktualisieren Sie die Mod auf die neueste Version, um Ergebnisse zu übermitteln." },
        { "hu", "Kérjük, frissítse a modot a legújabb verzióra az eredmények elküldéséhez." },
        { "it", "Aggiorna il mod all'ultima versione per inviare i risultati." },
        { "jp", "結果を送信するには、MODを最新バージョンに更新してください。" },
        { "kr", "결과를 제출하려면 모드를 최신 버전으로 업데이트하세요." },
        { "pl", "Zaktualizuj mod do najnowszej wersji, aby wysłać wyniki." },
        { "po", "Atualize o mod para a versão mais recente para enviar os resultados." },
        { "sk", "Aktualizujte mód na najnovšiu verziu pre odoslanie výsledkov." },
        { "es", "Actualiza el mod a la última versión para enviar los resultados." },
        { "es-mx", "Actualiza el mod a su última versión para poder enviar los resultados." },
        { "tu", "Sonuçları göndermek için lütfen modu en son sürüme güncelleyin." },
        { "ru", "Пожалуйста, обновите мод до последней версии для отправки результатов." },
        { "ro", "Vă rugăm să actualizați modul la cea mai recentă versiune pentru a trimite rezultatele." }
    };
    
    public static Dictionary<string, string> Error_ScavOnlyPublic = new Dictionary<string, string> // 231
    {
        { "ch", "仅当启用公开资料时，才能记录SCAV突袭。" },
        { "cz", "Záznam SCAV nájezdu je dostupný pouze s veřejným profilem." },
        { "en", "SCAV raid submission is only available with a public profile enabled." },
        { "fr", "L'enregistrement d'un raid SCAV est disponible uniquement avec un profil public activé." },
        { "ge", "Das Einreichen eines SCAV-Raids ist nur mit aktiviertem öffentlichen Profil möglich." },
        { "hu", "A SCAV rajtaütések mentése csak nyilvános profil esetén elérhető." },
        { "it", "La registrazione del raid SCAV è disponibile solo con un profilo pubblico abilitato." },
        { "jp", "SCAVレイドの記録は、公開プロフィールが有効な場合にのみ可能です。" },
        { "kr", "SCAV 레이드는 공개 프로필이 활성화된 경우에만 기록할 수 있습니다." },
        { "pl", "Zapis rajdu SCAV jest dostępny tylko przy włączonym profilu publicznym." },
        { "po", "O registo de raides SCAV só está disponível com o perfil público ativado." },
        { "sk", "Záznam SCAV nájazdu je dostupný iba pri zapnutom verejnom profile." },
        { "es", "El registro de una incursión SCAV solo está disponible con el perfil público activado." },
        { "es-mx", "Solo se puede registrar una incursión SCAV si el perfil público está activado." },
        { "tu", "SCAV baskını kaydı yalnızca herkese açık profil etkinleştirildiğinde kullanılabilir." },
        { "ru", "Запись рейда за SCAV доступна только при включённом публичном профиле." },
        { "ro", "Înregistrarea raidului SCAV este disponibilă doar cu un profil public activat." }
    };
    
    public static Dictionary<string, string> Error_CharLimit = new Dictionary<string, string> // 232
    {
        { "ch", "玩家名称过长。请在游戏设置中缩短。" },
        { "cz", "Jméno hráče je příliš dlouhé. Zkraťte ho v nastavení hry." },
        { "en", "The player name is too long. Please shorten it in the game settings." },
        { "fr", "Le nom du joueur est trop long. Veuillez le raccourcir dans les paramètres du jeu." },
        { "ge", "Der Spielername ist zu lang. Bitte kürzen Sie ihn in den Spieleinstellungen." },
        { "hu", "A játékos neve túl hosszú. Rövidítse le a játék beállításaiban." },
        { "it", "Il nome del giocatore è troppo lungo. Accorcialo nelle impostazioni di gioco." },
        { "jp", "プレイヤー名が長すぎます。ゲーム設定で短くしてください。" },
        { "kr", "플레이어 이름이 너무 깁니다. 게임 설정에서 짧게 줄여주세요." },
        { "pl", "Nazwa gracza jest zbyt długa. Skróć ją w ustawieniach gry." },
        { "po", "O nome do jogador é muito longo. Reduza-o nas configurações do jogo." },
        { "sk", "Meno hráča je príliš dlhé. Skráťte ho v nastaveniach hry." },
        { "es", "El nombre del jugador es demasiado largo. Acórtalo en la configuración del juego." },
        { "es-mx", "El nombre del jugador es muy largo. Modifícalo desde la configuración del juego." },
        { "tu", "Oyuncu adı çok uzun. Lütfen oyun ayarlarından kısaltın." },
        { "ru", "Имя игрока слишком длинное. Укоротите его в настройках игры." },
        { "ro", "Numele jucătorului este prea lung. Vă rugăm să-l scurtați din setările jocului." }
    };
    
    public static Dictionary<string, string> Error_NsfwName = new Dictionary<string, string> // 707
    {
        { "ch", "玩家名称包含禁止的字符或词语。请在游戏设置中修改。" },
        { "cz", "Jméno hráče obsahuje zakázané znaky nebo slova. Změňte ho v nastavení hry." },
        { "en", "The player name contains forbidden characters or words. Please change it in the game settings." },
        { "fr", "Le nom du joueur contient des caractères ou mots interdits. Veuillez le modifier dans les paramètres du jeu." },
        { "ge", "Der Spielername enthält unzulässige Zeichen oder Wörter. Bitte ändern Sie ihn in den Spieleinstellungen." },
        { "hu", "A játékos neve tiltott karaktereket vagy szavakat tartalmaz. Módosítsa a játék beállításaiban." },
        { "it", "Il nome del giocatore contiene caratteri o parole vietati. Cambialo nelle impostazioni di gioco." },
        { "jp", "プレイヤー名に禁止されている文字または単語が含まれています。ゲーム設定で変更してください。" },
        { "kr", "플레이어 이름에 금지된 문자나 단어가 포함되어 있습니다. 게임 설정에서 이름을 변경하세요." },
        { "pl", "Nazwa gracza zawiera niedozwolone znaki lub słowa. Zmień ją w ustawieniach gry." },
        { "po", "O nome do jogador contém caracteres ou palavras proibidas. Altere-o nas configurações do jogo." },
        { "sk", "Meno hráča obsahuje zakázané znaky alebo slová. Zmeňte ho v nastaveniach hry." },
        { "es", "El nombre del jugador contiene caracteres o palabras prohibidas. Modifícalo en la configuración del juego." },
        { "es-mx", "El nombre del jugador tiene caracteres o palabras no permitidas. Cámbialo en la configuración del juego." },
        { "tu", "Oyuncu adı yasaklı karakterler veya kelimeler içeriyor. Lütfen oyun ayarlarından değiştirin." },
        { "ru", "Имя игрока содержит запрещённые символы или слова. Измените имя в настройках игры." },
        { "ro", "Numele jucătorului conține caractere sau cuvinte interzise. Vă rugăm să-l modificați din setările jocului." }
    };
    
    public static Dictionary<string, string> Error_DevItems = new Dictionary<string, string>
    {
        { "ch", "检测到开发者物品。突袭数据将不会被上传。" },
        { "cz", "Byl u vás nalezen vývojářský předmět. Údaje z nájezdu nebudou odeslány." },
        { "en", "Developer item detected. Raid data will not be submitted." },
        { "fr", "Un objet de développeur a été détecté. Les données du raid ne seront pas envoyées." },
        { "ge", "Ein Entwicklergegenstand wurde gefunden. Raid-Daten werden nicht gesendet." },
        { "hu", "Fejlesztői tárgyat észleltünk. A rajtaütés adatai nem kerülnek elküldésre." },
        { "it", "Oggetto sviluppatore rilevato. I dati del raid non verranno inviati." },
        { "jp", "開発者アイテムが検出されました。レイドデータは送信されません。" },
        { "kr", "개발자 아이템이 감지되었습니다. 레이드 데이터는 전송되지 않습니다." },
        { "pl", "Wykryto przedmiot dewelopera. Dane z rajdu nie zostaną wysłane." },
        { "po", "Item de desenvolvedor detectado. Os dados da incursão não serão enviados." },
        { "sk", "Bol zistený vývojársky predmet. Údaje z nájazdu nebudú odoslané." },
        { "es", "Se ha detectado un objeto de desarrollador. Los datos de la incursión no se enviarán." },
        { "es-mx", "Se detectó un objeto de desarrollador. Los datos del asalto no se enviarán." },
        { "tu", "Geliştirici öğesi tespit edildi. Baskın verileri gönderilmeyecek." },
        { "ru", "У вас обнаружен предмет разработчика. Данные рейда не будут отправлены." },
        { "ro", "A fost detectat un obiect de dezvoltator. Datele raidului nu vor fi trimise." }
    };
    
    // Template for translating
    // public static Dictionary<string, string> TEMPLATE = new Dictionary<string, string>
    // {
    //     { "ch", "" },
    //     { "cz", "" },
    //     { "en", "" },
    //     { "fr", "" },
    //     { "ge", "" },
    //     { "hu", "" },
    //     { "it", "" },
    //     { "jp", "" },
    //     { "kr", "" },
    //     { "pl", "" },
    //     { "po", "" },
    //     { "sk", "" },
    //     { "es", "" },
    //     { "es-mx", "" },
    //     { "tu", "" },
    //     { "ru", "" },
    //     { "ro", "" }
    // };
}