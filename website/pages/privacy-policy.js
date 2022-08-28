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
        <h1>Privacy Policy</h1>
      </section>
    </Layout>
  )
}
