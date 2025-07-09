using System.Collections.Generic;

namespace SPTLeaderboard.Data;

public class LocalizationData
{
    public static Dictionary<string, string> ErrorBanned = new Dictionary<string, string>
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
        
    public static Dictionary<string, string> ErrorRules = new Dictionary<string, string>
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
}