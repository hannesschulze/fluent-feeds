import Layout from '../components/layout';
import Section from '../components/section';
import Head from 'next/head';

export default function Home() {
  return (
    <Layout>
      <Head>
        <title>Fluent Feeds</title>
      </Head>

      <Section>
        <h1>Home</h1>
      </Section>
    </Layout>
  )
}
