import Layout from '../components/layout';
import Head from 'next/head';
import commonStyles from '../styles/common.module.css';

export default function PrivacyPolicy() {
  return (
    <Layout>
      <Head>
        <title>Privacy Policy | Fluent Feeds</title>
      </Head>

      <section className={commonStyles.section}>
        <h1>Privacy Policy/<wbr />Datenschutz<wbr />erklärung</h1>

        <p>
          Im Folgenden möchten wir Sie aufklären, wie Ihre Daten von uns verarbeitet werden.
        </p>
        
        <p>
          <strong>Verantwortlich im Sinne der DSGVO ist:</strong><br />
          Hannes Schulze<br />
          Singer Str. 79<br />
          98693 Ilmenau<br />
          haschu0103(at)gmail.com
        </p>

        <p>
          Sollten Sie per E-Mail mit uns Kontakt aufnehmen, werden die mitgeteilten Daten von uns gespeichert, um Ihr
          Anliegen zu bearbeiten.
        </p>
        
        <p>
          <strong>Zu den verarbeiteten Daten zählen:</strong><br />
          Ihr Name<br />
          Ihre E-Mail-Adresse
        </p>

        <p>
          Wir werden die Daten löschen, sobald die Speicherung nicht mehr erforderlich ist oder die Verarbeitung
          einschränken, falls gesetzliche Aufbewahrungspflichten bestehen.
        </p>

        <h2>Betroffenenrechte</h2>

        <p>
          Sie haben als betroffene Person, das Recht auf Auskunft, das Recht auf Berichtigung oder Löschung, das Recht
          auf Einschränkung der Verarbeitung und das Recht auf Widerspruch gegen die Verarbeitung Ihrer Daten. Sofern
          Sie uns eine Einwilligung erteilt haben, können Sie diese jederzeit mit Wirkung für die Zukunft widerrufen.
        </p>

        <p>
          Bitte richten Sie Ihren Widerspruch formlos an die oben genannte Adresse.
        </p>

        <p>
          Darüber hinaus haben Sie das Recht auf Datenübertragbarkeit. Sie haben weiter das Recht, sich bei einer
          Aufsichtsbehörde über die Verarbeitung zu beschweren. Eine Liste der entsprechenden Behörden finden Sie
          unter: <a href='https://www.bfdi.bund.de/DE/Infothek/Anschriften_Links/anschriften_links-node.html' className='breaking'>
          https://www.bfdi.bund.de/DE/Infothek/Anschriften_Links/anschriften_links-node.html</a>.
        </p>

        <h2>Hosting</h2>
        
        <p>
          Unser Hoster erhebt in sog. Logfiles folgende Daten, die Ihr Browser übermittelt:
        </p>

        <p>
          IP-Adresse, die Adresse der vorher besuchten Website (Referer Anfrage-Header), Datum und Uhrzeit der Anfrage,
          Zeitzonendifferenz zur Greenwich Mean Time, Inhalt der Anforderung, HTTP-Statuscode, übertragene Datenmenge,
          Website, von der die Anforderung kommt und Informationen zu Browser und Betriebssystem.
        </p>

        <p>
          Das ist erforderlich, um unsere Website anzuzeigen und die Stabilität und Sicherheit zu gewährleisten. Dies
          entspricht unserem berechtigten Interesse im Sinne des Art. 6 Abs. 1 S. 1 lit. f DSGVO.
        </p>

        <p>
          Es erfolgt kein Tracking und wir haben auf diese Daten keinen direkten Zugriff, sondern erhalten lediglich
          eine anonymisierte, statistische Zusammenfassung. Diese beinhaltet die Adresse der vorher besuchten Seite,
          die Häufigkeit der jeweils aufgerufenen Seiten und die Anzahl eindeutiger Besucher. Diese Daten führen wir
          nicht mit anderen Daten zusammen.
        </p>

        <p>
          Wir setzen für die Zurverfügungstellung unserer Website folgenden Hoster ein:
        </p>

        <p>
          GitHub Inc.<br />
          88 Colin P Kelly Jr St<br />
          San Francisco, CA 94107<br />
          United States
        </p>

        <p>
          Dieser ist Empfänger Ihrer personenbezogenen Daten. Dies entspricht unserem berechtigten Interesse im Sinne
          des Art. 6 Abs. 1 S. 1 lit. f DSGVO, selbst keinen Server in unseren Räumlichkeiten vorhalten zu müssen.
          Serverstandort ist USA.
        </p>

        <p>
          Weitere Informationen zu Widerspruchs- und Beseitigungsmöglichkeiten gegenüber GitHub finden Sie
          unter: <a href='https://docs.github.com/en/free-pro-team@latest/github/site-policy/github-privacy-statement#github-pages' className='breaking'>
          https://docs.github.com/en/free-pro-team@latest/github/site-policy/github-privacy-statement#github-pages</a>
        </p>

        <p>
          Sie haben das Recht der Verarbeitung zu widersprechen. Ob der Widerspruch erfolgreich ist, ist im Rahmen
          einer Interessenabwägung zu ermitteln.
        </p>

        <p>
          Die Daten werden gelöscht, sobald der Zweck der Verarbeitung entfällt.
        </p>

        <p>
          Die Verarbeitung der unter diesem Abschnitt angegebenen Daten ist weder gesetzlich noch vertraglich
          vorgeschrieben. Die Funktionsfähigkeit der Website ist ohne die Verarbeitung nicht gewährleistet.
        </p>

        <p>
          GitHub hat Compliance-Maßnahmen für internationale Datenübermittlungen umgesetzt. Diese gelten für alle
          weltweiten Aktivitäten, bei denen GitHub personenbezogene Daten von natürlichen Personen in der EU
          verarbeitet. Diese Maßnahmen basieren auf den EU-Standardvertragsklauseln (SCCs). Weitere Informationen
          finden Sie
          unter: <a href='https://docs.github.com/en/free-pro-team@latest/github/site-policy/github-data-protection-addendum#attachment-1–the-standard-contractual-clauses-processors' className='breaking'>
          https://docs.github.com/en/free-pro-team@latest/github/site-policy/github-data-protection-addendum#attachment-1–the-standard-contractual-clauses-processors</a>
        </p>
      </section>
    </Layout>
  )
}
